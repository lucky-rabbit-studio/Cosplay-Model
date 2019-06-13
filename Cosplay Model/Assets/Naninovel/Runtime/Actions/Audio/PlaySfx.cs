// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Plays or modifies currently played SFX (sound effect) track with the provided name.
    /// </summary>
    /// <remarks>
    /// Sound effect tracks are not looped by default.
    /// When sfx track name (SfxPath) is not specified, will affect all the currently played tracks.
    /// When invoked for a track that is already playing, the playback won't be affected (track won't start playing from the start),
    /// but the specified parameters (volume and whether the track is looped) will be applied.
    /// </remarks>
    /// <example>
    /// ; Plays an SFX with the name `Explosion` once
    /// @sfx Explosion
    /// 
    /// ; Plays an SFX with the name `Rain` in a loop
    /// @sfx Rain loop:true
    /// 
    /// ; Changes volume of all the played SFX tracks to 75% over 2.5 seconds and disables looping for all of them
    /// @sfx volume:0.75 loop:false time:2.5
    /// </example>
    [ActionAlias("sfx")]
    public class PlaySfx : NovelAction, NovelAction.IPreloadable
    {
        private struct UndoData { public bool Executed; public List<AudioManager.ClipState> SfxState; }

        /// <summary>
        /// Path to the sound effect asset to play.
        /// </summary>
        [ActionParameter(NamelessParameterAlias, true)]
        public string SfxPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Volume of the sound effect.
        /// </summary>
        [ActionParameter(optional: true)]
        public float Volume { get => GetDynamicParameter(1f); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to play the sound effect in a loop.
        /// </summary>
        [ActionParameter(optional: true)]
        public bool Loop { get => GetDynamicParameter(false); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public async Task PreloadResourcesAsync ()
        {
            if (string.IsNullOrWhiteSpace(SfxPath)) return;
            await Engine.GetService<AudioManager>()?.PreloadAudioAsync(SfxPath);
        }

        public async Task UnloadResourcesAsync ()
        {
            if (string.IsNullOrWhiteSpace(SfxPath)) return;
            await Engine.GetService<AudioManager>()?.UnloadAudioAsync(SfxPath);
        }

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<AudioManager>();
            var allSfxState = manager.CloneAllPlayingSfxState();

            undoData.Executed = true;
            undoData.SfxState = allSfxState;

            if (string.IsNullOrWhiteSpace(SfxPath))
                await Task.WhenAll(allSfxState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, Volume, Loop, Duration)));
            else await PlayOrModifyTrackAsync(manager, SfxPath, Volume, Loop, Duration);
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var manager = Engine.GetService<AudioManager>();
            manager.StopAllSfx();
            await Task.WhenAll(undoData.SfxState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, s.Volume, s.IsLooped, 0)));

            undoData = default;
        }

        private static async Task PlayOrModifyTrackAsync (AudioManager mngr, string path, float volume, bool loop, float time)
        {
            if (mngr.IsSfxPlaying(path)) mngr.ModifySfx(path, volume, loop, time);
            else await mngr.PlaySfxAsync(path, volume, time, loop);
        }
    } 
}
