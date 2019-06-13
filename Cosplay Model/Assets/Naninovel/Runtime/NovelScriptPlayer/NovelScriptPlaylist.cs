// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Represents a list of <see cref="NovelAction"/> based on the contents of a <see cref="NovelScript"/>.
    /// </summary>
    public class NovelScriptPlaylist : List<NovelAction>
    {
        public string ScriptName { get; }

        public NovelScriptPlaylist (NovelScript script)
        {
            ScriptName = script.Name;
            var actions = script.CollectAllActionLines().Select(l => NovelAction.FromScriptLine(l)).Where(a => a != null);
            AddRange(actions);
        }

        /// <summary>
        /// Preloads resources required to execute <see cref="NovelAction.IPreloadable"/> actions.
        /// </summary>
        public async Task PreloadActionsAsync (int startActionIndex = 0)
        {
            if (!this.IsIndexValid(startActionIndex)) return;

            var actionsToPreload = GetRange(startActionIndex, Count - startActionIndex).OfType<NovelAction.IPreloadable>();
            // Could lead to race conditions when creating actors in action's preload methods.
            // await Task.WhenAll(actionsToPreload.Select(a => a.PreloadResourcesAsync()));
            foreach (var action in actionsToPreload)
                await action.PreloadResourcesAsync();
        }

        /// <summary>
        /// Unloads resources loaded with <see cref="PreloadActionsAsync"/>.
        /// </summary>
        public async Task UnloadActionsAsync ()
        {
            var actionsToUnload = this.OfType<NovelAction.IPreloadable>();
            await Task.WhenAll(actionsToUnload.Select(a => a.UnloadResourcesAsync()));
        }

        /// <summary>
        /// Returns a <see cref="NovelAction"/> at the provided index; null if not found.
        /// </summary>
        public NovelAction GetActionByIndex (int actionIndex)
        {
            if (!this.IsIndexValid(actionIndex)) return null;
            return this[actionIndex];
        }

        /// <summary>
        /// Finds a <see cref="NovelAction"/> that was created from a <see cref="ActionScriptLine"/> with provided line and inline indexes.
        /// </summary>
        public NovelAction GetActionByLine (int lineIndex, int inlineIndex)
        {
            return Find(a => a.LineIndex == lineIndex && a.InlineIndex == inlineIndex);
        }

        /// <summary>
        /// Finds a <see cref="NovelAction"/> that was created from a <see cref="ActionScriptLine"/> located at or after provided line and inline indexes.
        /// </summary>
        public NovelAction GetFirstActionAfterLine (int lineIndex, int inlineIndex)
        {
            return Find(a => a.LineIndex >= lineIndex && a.InlineIndex >= inlineIndex);
        }
    }
}
