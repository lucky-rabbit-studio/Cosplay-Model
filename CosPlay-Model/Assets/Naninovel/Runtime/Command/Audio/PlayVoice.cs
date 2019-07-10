// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Plays a voice clip at the provided path.
    /// </summary>
    [CommandAlias("voice")]
    public class PlayVoice : Command, Command.IPreloadable
    {
        /// <summary>
        /// Path to the voice clip to play.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string VoicePath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Volume of the playback.
        /// </summary>
        [CommandParameter(optional: true)]
        public float Volume { get => GetDynamicParameter(1f); set => SetDynamicParameter(value); }

        public async Task HoldResourcesAsync ()
        {
            await Engine.GetService<AudioManager>()?.HoldVoiceResourcesAsync(this, VoicePath);
        }

        public void ReleaseResources ()
        {
            Engine.GetService<AudioManager>()?.ReleaseVoiceResources(this, VoicePath);
        }

        public override async Task ExecuteAsync ()
        {
            await Engine.GetService<AudioManager>()?.PlayVoiceAsync(VoicePath, Volume);
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
