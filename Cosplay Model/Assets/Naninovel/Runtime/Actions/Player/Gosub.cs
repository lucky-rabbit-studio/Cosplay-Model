// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Actions
{
    /// <summary>
    /// Jumps the novel script playback to the provided path and saves the path to the global state; 
    /// [`@return`](/api/#return) actions use this info to redirect to action after the last invoked gosub action. 
    /// Useful for invoking a repeating set of actions multiple times.
    /// </summary>
    /// <example>
    /// ; Jumps the playback to the label `VictoryScene` in the currently played script,
    /// ; executes the actions and jumps back to the action after the `gosub`.
    /// @gosub .VictoryScene
    /// ...
    /// @stop
    /// 
    /// # VictoryScene
    /// @back Victory
    /// @sfx Fireworks
    /// @bgm Fanfares
    /// You are victorious!
    /// @return
    /// </example>
    public class Gosub : NovelAction
    {
        private struct UndoData { public bool Executed; }

        /// <summary>
        /// Path to jump into in the following format: `ScriptName.LabelName`.
        /// When label name is ommited, will play provided script from the start.
        /// When script name is ommited, will attempt to find a label in the currently played script.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public Named<string> Path { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var player = Engine.GetService<NovelScriptPlayer>();

            undoData.Executed = true;

            var spot = new PlaybackSpot {
                ScriptName = player.PlayedScript?.Name,
                LineIndex = player.PlayedAction?.LineIndex + 1 ?? 0,
            };
            player.LastGosubReturnSpots.Push(spot);

            await new Goto { Path = Path }.ExecuteAsync();
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var player = Engine.GetService<NovelScriptPlayer>();
            player.LastGosubReturnSpots.Pop();

            undoData = default;
            return Task.CompletedTask;
        }
    } 
}
