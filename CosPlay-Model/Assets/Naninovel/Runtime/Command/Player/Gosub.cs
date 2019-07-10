// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Commands
{
    /// <summary>
    /// Jumps the naninovel script playback to the provided path and saves the path to the global state; 
    /// [`@return`](/api/#return) commands use this info to redirect to command after the last invoked gosub command. 
    /// Resembles a function (subroutine) in a programming language, allowing to reuse a piece of naninovel script.
    /// Useful for invoking a repeating set of commands multiple times.
    /// </summary>
    /// <example>
    /// ; Jumps the playback to the label `VictoryScene` in the currently played script,
    /// ; executes the commands and jumps back to the command after the `gosub`.
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
    /// 
    /// ; Another example with some branching inside the subroutine.
    /// @set time=10
    /// ; Here we get one result
    /// @gosub .Room
    /// ...
    /// @set time=3
    /// ; And here we get another
    /// @gosub .Room
    /// ...
    /// 
    /// # Room
    /// @print "It's too early, I should visit this place when it's dark." if:time&lt;21&amp;&amp;time>6
    /// @print "I can sense an ominous presence here!" if:time>21&amp;&amp;time&lt;6
    /// @return
    /// </example>
    public class Gosub : Command
    {
        private struct UndoData { public bool Executed; }

        /// <summary>
        /// Path to jump into in the following format: `ScriptName.LabelName`.
        /// When label name is ommited, will play provided script from the start.
        /// When script name is ommited, will attempt to find a label in the currently played script.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public Named<string> Path { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var player = Engine.GetService<ScriptPlayer>();

            undoData.Executed = true;

            var spot = new PlaybackSpot {
                ScriptName = player.PlayedScript?.Name,
                LineIndex = player.PlayedCommand?.LineIndex + 1 ?? 0,
            };
            player.LastGosubReturnSpots.Push(spot);

            await new Goto { Path = Path }.ExecuteAsync();
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var player = Engine.GetService<ScriptPlayer>();
            player.LastGosubReturnSpots.Pop();

            undoData = default;
            return Task.CompletedTask;
        }
    } 
}
