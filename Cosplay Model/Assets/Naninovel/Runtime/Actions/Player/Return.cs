// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Attempts to jump the novel script playback to the action after the last used @gosub. 
    /// See [`@gosub`](/api/#gosub) action summary for more info.
    /// </summary>
    public class Return : NovelAction
    {
        private struct UndoData { public bool Executed; public PlaybackSpot PoppedSpot; }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var player = Engine.GetService<NovelScriptPlayer>();

            if (player.LastGosubReturnSpots.Count == 0 || string.IsNullOrWhiteSpace(player.LastGosubReturnSpots.Peek().ScriptName))
            {
                Debug.LogWarning("Failed to return to the last gosub: state data is missing or invalid.");
                return;
            }

            var spot = player.LastGosubReturnSpots.Pop();

            undoData.Executed = true;
            undoData.PoppedSpot = spot;

            if (player.PlayedScript != null && player.PlayedScript.Name.EqualsFastIgnoreCase(spot.ScriptName))
            {
                player.Play(player.PlayedScript, spot.LineIndex);
                return;
            }

            var stateManager = Engine.GetService<StateManager>();
            await stateManager?.LoadDefaultEngineStateAsync(() => player.PreloadAndPlayAsync(spot.ScriptName, spot.LineIndex));
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var player = Engine.GetService<NovelScriptPlayer>();
            player.LastGosubReturnSpots.Push(undoData.PoppedSpot);

            undoData = default;
            return Task.CompletedTask;
        }
    } 
}
