// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a list of <see cref="Command"/> based on the contents of a <see cref="Script"/>.
    /// </summary>
    public class ScriptPlaylist : List<Command>
    {
        /// <summary>
        /// Name of the script from which the contained commands were extracted.
        /// </summary>
        public string ScriptName { get; }

        public ScriptPlaylist (Script script)
        {
            ScriptName = script.Name;
            var commands = script.CollectAllCommandLines()
                .Select(l => Command.FromScriptLine(l))
                .Where(cmd => cmd != null);
            AddRange(commands);
        }

        /// <summary>
        /// Preloads and holds all the resources required to execute <see cref="Command.IPreloadable"/> commands contained in this list.
        /// </summary>
        public async Task HoldResourcesAsync () => await HoldResourcesAsync(0, Count - 1);

        /// <summary>
        /// Preloads and holds resources required to execute <see cref="Command.IPreloadable"/> commands in the specified range.
        /// </summary>
        public async Task HoldResourcesAsync (int startCommandIndex, int endCommandIndex)
        {
            if (Count == 0) return;

            if (!this.IsIndexValid(startCommandIndex) || !this.IsIndexValid(endCommandIndex) || endCommandIndex < startCommandIndex)
            {
                Debug.LogError($"Failed to preload `{ScriptName}` script resources: [{startCommandIndex}, {endCommandIndex}] is not a valid range.");
                return;
            }

            var commandsToHold = GetRange(startCommandIndex, (endCommandIndex + 1) - startCommandIndex).OfType<Command.IPreloadable>();
            await Task.WhenAll(commandsToHold.Select(cmd => cmd.HoldResourcesAsync()));
        }

        /// <summary>
        /// Releases all the held resources required to execute <see cref="Command.IPreloadable"/> commands contained in this list.
        /// </summary>
        public void ReleaseResources () => ReleaseResources(0, Count - 1);

        /// <summary>
        /// Releases all the held resources required to execute <see cref="Command.IPreloadable"/> commands in the specified range.
        /// </summary>
        public void ReleaseResources (int startCommandIndex, int endCommandIndex)
        {
            if (Count == 0) return;

            if (!this.IsIndexValid(startCommandIndex) || !this.IsIndexValid(endCommandIndex) || endCommandIndex < startCommandIndex)
            {
                Debug.LogError($"Failed to unload `{ScriptName}` script resources: [{startCommandIndex}, {endCommandIndex}] is not a valid range.");
                return;
            }

            var commandsToRelease = GetRange(startCommandIndex, (endCommandIndex + 1) - startCommandIndex).OfType<Command.IPreloadable>();
            foreach (var cmd in commandsToRelease)
                cmd.ReleaseResources();
        }

        /// <summary>
        /// Returns a <see cref="Command"/> at the provided index; null if not found.
        /// </summary>
        public Command GetCommandByIndex (int commandIndex) => this.IsIndexValid(commandIndex) ? this[commandIndex] : null;

        /// <summary>
        /// Finds a <see cref="Command"/> that was created from a <see cref="CommandScriptLine"/> with provided line and inline indexes.
        /// </summary>
        public Command GetCommandByLine (int lineIndex, int inlineIndex) => Find(a => a.LineIndex == lineIndex && a.InlineIndex == inlineIndex);

        /// <summary>
        /// Finds a <see cref="Command"/> that was created from a <see cref="CommandScriptLine"/> located at or after provided line and inline indexes.
        /// </summary>
        public Command GetFirstCommandAfterLine (int lineIndex, int inlineIndex) => Find(a => a.LineIndex >= lineIndex && a.InlineIndex >= inlineIndex);
    }
}
