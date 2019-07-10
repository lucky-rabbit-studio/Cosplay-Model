// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Commands
{
    /// <summary>
    /// Jumps the naninovel script playback to the provided path.
    /// When the path leads to another (not the currently played) naninovel script, will also [reset state](/api/#resetstate) 
    /// before loading the target script, unless [ResetStateOnLoad](https://naninovel.com/guide/configuration.html#state) is disabled in the configuration.
    /// </summary>
    /// <example>
    /// ; Loads and starts playing a naninovel script with the name `Script001` from the start.
    /// @goto Script001
    /// 
    /// ; Save as above, but start playing from the label `AfterStorm`.
    /// @goto Script001.AfterStorm
    /// 
    /// ; Jumps the playback to the label `Epilogue` in the currently played script.
    /// @goto .Epilogue
    /// </example>
    public class Goto : Command
    {
        /// <summary>
        /// Path to jump into in the following format: `ScriptName.LabelName`.
        /// When label name is ommited, will play provided script from the start.
        /// When script name is ommited, will attempt to find a label in the currently played script.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public Named<string> Path { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            var player = Engine.GetService<ScriptPlayer>();

            var scriptName = Path.Item1;
            var labelName = Path.Item2;

            // Just jump to a label inside current script.
            if (string.IsNullOrWhiteSpace(scriptName) || scriptName.EqualsFastIgnoreCase(player.PlayedScript.Name))
            {
                player.Play(player.PlayedScript, labelName);
                return;
            }

            // Load another script and start playing from label.
            var stateManager = Engine.GetService<StateManager>();
            if (stateManager.ResetStateOnLoad)
                await stateManager?.ResetStateAsync(() => player.PreloadAndPlayAsync(scriptName, label: labelName));
            else await player.PreloadAndPlayAsync(scriptName, label: labelName);
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
