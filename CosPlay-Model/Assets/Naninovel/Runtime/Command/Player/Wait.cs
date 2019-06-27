// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Holds script execution until the specified wait condition.
    /// </summary>
    /// <example>
    /// ; "ThunderSound" SFX will play 0.5 seconds after the shake background effect finishes.
    /// @fx ShakeBackground
    /// @wait 0.5
    /// @sfx ThunderSound
    /// 
    /// ; Print first two words, then wait for user input before printing the remaining phrase.
    /// Lorem ipsum[wait i] dolor sit amet.
    /// ; You can also use the following shortcut (@i command) for this wait mode.
    /// Lorem ipsum[i] dolor sit amet.
    /// 
    /// ; Start an SFX, print a message and wait for a skippable 5 seconds delay, then stop the SFX.
    /// @sfx Noise loop:true
    /// Jeez, what a disgusting noise. Shut it down![wait i5][skipInput]
    /// @stopSfx Noise
    /// </example>
    public class Wait : Command
    {
        /// <summary>
        /// Literal used to indicate "wait-for-input" mode.
        /// </summary>
        public const string InputLiteral = "i";

        /// <summary>
        /// Wait conditions:<br/>
        ///  - `i` user press continue or skip input key;<br/>
        ///  - `0.0` timer (seconds);<br/>
        ///  - `i0.0` timer, that is skippable by continue or skip input keys.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string WaitMode { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private static readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private static InputManager inputManager => inputManagerCache ?? (inputManagerCache = Engine.GetService<InputManager>());
        private static ScriptPlayer scriptPlayer => scriptPlayerCache ?? (scriptPlayerCache = Engine.GetService<ScriptPlayer>());
        private static InputManager inputManagerCache;
        private static ScriptPlayer scriptPlayerCache;

        public override async Task ExecuteAsync ()
        {
            if (scriptPlayer.IsSkipActive) return;

            // Waiting for player input.
            if (WaitMode.EqualsFastIgnoreCase(InputLiteral))
            {
                scriptPlayer.EnableWaitingForInput();
                return;
            }

            // Waiting for timer or input.
            if (WaitMode.StartsWithFast(InputLiteral) && ParseUtils.TryInvariantFloat(WaitMode.GetAfterFirst(InputLiteral), out var skippableWaitTime))
            {
                scriptPlayer.EnableWaitingForInput();
                var startTime = Time.time;
                while (Application.isPlaying)
                {
                    await waitForEndOfFrame;
                    var waitedEnough = (Time.time - startTime) >= skippableWaitTime;
                    var inputActivated = inputManager.Continue.StartedDuringFrame || inputManager.Skip.StartedDuringFrame;
                    if (waitedEnough || inputActivated) break;
                }
                scriptPlayer.DisableWaitingForInput();
                return;
            }

            // Waiting for timer.
            if (ParseUtils.TryInvariantFloat(WaitMode, out var waitTime))
            {
                var startTime = Time.time;
                while (Application.isPlaying)
                {
                    await waitForEndOfFrame;
                    var waitedEnough = (Time.time - startTime) >= waitTime;
                    if (waitedEnough) break;
                }
                return;
            }

            Debug.LogWarning($"Failed to determine wait mode for the wait command in script '{ScriptName}' at line #'{LineNumber}'.");
        }

        public override Task UndoAsync ()
        {
            if (WaitMode.EqualsFastIgnoreCase(InputLiteral))
                scriptPlayer.DisableWaitingForInput();

            return Task.CompletedTask;
        }
    } 
}
