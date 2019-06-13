// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Plays or modifies currently played BGM (background music) track with the provided name.
    /// </summary>
    /// <remarks>
    /// Music tracks are looped by default.
    /// When music track name (BgmPath) is not specified, will affect all the currently played tracks.
    /// When invoked for a track that is already playing, the playback won't be affected (track won't start playing from the start),
    /// but the specified parameters (volume and whether the track is looped) will be applied.
    /// </remarks>
    /// <example>
    /// ; Fades-in a music track with the name `Sanctuary` over default fade duration and plays it in a loop
    /// @bgm Sanctuary
    /// 
    /// ; Same as above, but fade-in duration is 10 seconds and plays only once
    /// @bgm Sanctuary time:10 loop:false
    /// 
    /// ; Changes volume of all the played music tracks to 50% over 2.5 seconds and makes them play in a loop
    /// @bgm volume:0.5 loop:true time:2.5
    /// </example>
    [ActionAlias("bgm")]
    public class PlayBgm : NovelAction, NovelAction.IPreloadable
    {
        private struct UndoData { public bool Executed; public List<AudioManager.ClipState> BgmState; }

        /// <summary>
        /// Path to the music track to play.
        /// </summary>
        [ActionParameter(NamelessParameterAlias, true)]
        public string BgmPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Volume of the music track.
        /// </summary>
        [ActionParameter(optional: true)]
        public float Volume { get => GetDynamicParameter(1f); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to play the track from beginning when it finishes.
        /// </summary>
        [ActionParameter(optional: true)]
        public bool Loop { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public async Task PreloadResourcesAsync ()
        {
            if (string.IsNullOrWhiteSpace(BgmPath)) return;
            await Engine.GetService<AudioManager>()?.PreloadAudioAsync(BgmPath);
        }

        public async Task UnloadResourcesAsync ()
        {
            if (string.IsNullOrWhiteSpace(BgmPath)) return;
            await Engine.GetService<AudioManager>()?.UnloadAudioAsync(BgmPath);
        }

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<AudioManager>();
            var allBgmState = manager.CloneAllPlayingBgmState();

            undoData.Executed = true;
            undoData.BgmState = allBgmState;

            if (string.IsNullOrWhiteSpace(BgmPath))
                await Task.WhenAll(allBgmState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, Volume, Loop, Duration)));
            else await PlayOrModifyTrackAsync(manager, BgmPath, Volume, Loop, Duration);
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var manager = Engine.GetService<AudioManager>();
            manager.StopAllBgm();
            await Task.WhenAll(undoData.BgmState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, s.Volume, s.IsLooped, 0)));

            undoData = default;
        }

        private static async Task PlayOrModifyTrackAsync (AudioManager mngr, string path, float volume, bool loop, float time)
        {
            if (mngr.IsBgmPlaying(path)) mngr.ModifyBgm(path, volume, loop, time);
            else await mngr.PlayBgmAsync(path, volume, time, loop);
        }
    } 
}
