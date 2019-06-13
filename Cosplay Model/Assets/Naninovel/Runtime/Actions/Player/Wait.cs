// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Actions
{
    /// <summary>
    /// Holds script execution until the specified wait condition.
    /// </summary>
    /// <example>
    /// ; `ThunderSound` SFX will play 0.5 seconds after the shake background effect finishes.
    /// @fx ShakeBackground
    /// @wait 0.5
    /// @sfx ThunderSound
    /// </example>
    public class Wait : NovelAction
    {
        /// <summary>
        /// Wait condition: 
        ///     input (string) - user press continue or skip input key;
        ///     number (int or float)  - timer (seconds).
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string WaitMode { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            // Waiting for player input.
            if (WaitMode.EqualsFastIgnoreCase("input"))
            {
                Engine.GetService<NovelScriptPlayer>()?.EnableWaitingForInput();
                return;
            }

            // Waiting for timer.
            if (ParseUtils.TryInvariantFloat(WaitMode, out var timeToWait))
            {
                await new Timer(timeToWait).RunAsync();
                return;
            }
        }

        public override Task UndoAsync ()
        {
            if (WaitMode.EqualsFastIgnoreCase("input"))
                Engine.GetService<NovelScriptPlayer>()?.DisableWaitingForInput();

            return Task.CompletedTask;
        }
    } 
}
