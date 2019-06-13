// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Actions
{
    /// <summary>
    /// Jumps the novel script playback to the provided path.
    /// </summary>
    /// <example>
    /// ; Loads and starts playing a novel script with the name `Script001` from the start.
    /// @goto Script001
    /// 
    /// ; Save as above, but start playing from the label `AfterStorm`.
    /// @goto Script001.AfterStorm
    /// 
    /// ; Jumps the playback to the label `Epilogue` in the currently played script.
    /// @goto .Epilogue
    /// </example>
    public class Goto : NovelAction
    {
        /// <summary>
        /// Path to jump into in the following format: `ScriptName.LabelName`.
        /// When label name is ommited, will play provided script from the start.
        /// When script name is ommited, will attempt to find a label in the currently played script.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public Named<string> Path { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            var player = Engine.GetService<NovelScriptPlayer>();

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
            await stateManager?.LoadDefaultEngineStateAsync(() => player.PreloadAndPlayAsync(scriptName, label: labelName));
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
