// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Plays a voice clip at the provided path.
    /// </summary>
    [ActionAlias("voice")]
    public class PlayVoice : NovelAction, NovelAction.IPreloadable
    {
        /// <summary>
        /// Path to the voice clip to play.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string VoicePath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Volume of the playback.
        /// </summary>
        [ActionParameter(optional: true)]
        public float Volume { get => GetDynamicParameter(1f); set => SetDynamicParameter(value); }

        public async Task PreloadResourcesAsync ()
        {
            await Engine.GetService<AudioManager>()?.PreloadVoiceAsync(VoicePath);
        }

        public async Task UnloadResourcesAsync ()
        {
            await Engine.GetService<AudioManager>()?.UnloadVoiceAsync(VoicePath);
        }

        public override async Task ExecuteAsync ()
        {
            await Engine.GetService<AudioManager>()?.PlayVoiceAsync(VoicePath, Volume);
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
