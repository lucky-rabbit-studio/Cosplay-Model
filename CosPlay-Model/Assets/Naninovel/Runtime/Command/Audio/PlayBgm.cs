// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Commands
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
    /// 
    /// ; Playes `BattleThemeIntro` once and then immediately `BattleThemeMain` in a loop.
    /// @bgm BattleThemeMain intro:BattleThemeIntro
    /// </example>
    [CommandAlias("bgm")]
    public class PlayBgm : Command, Command.IPreloadable
    {
        private struct UndoData { public bool Executed; public List<AudioManager.ClipState> BgmState; }

        /// <summary>
        /// Path to the music track to play.
        /// </summary>
        [CommandParameter(NamelessParameterAlias, true)]
        public string BgmPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Path to the intro music track to play once before the main track (not affected by the loop parameter).
        /// </summary>
        [CommandParameter("intro", true)]
        public string IntroBgmPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Volume of the music track.
        /// </summary>
        [CommandParameter(optional: true)]
        public float Volume { get => GetDynamicParameter(1f); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to play the track from beginning when it finishes.
        /// </summary>
        [CommandParameter(optional: true)]
        public bool Loop { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public async Task HoldResourcesAsync ()
        {
            if (string.IsNullOrWhiteSpace(BgmPath)) return;
            await Engine.GetService<AudioManager>()?.HoldAudioResourcesAsync(this, BgmPath);

            if (string.IsNullOrWhiteSpace(IntroBgmPath)) return;
            await Engine.GetService<AudioManager>()?.HoldAudioResourcesAsync(this, IntroBgmPath);
        }

        public void ReleaseResources ()
        {
            if (string.IsNullOrWhiteSpace(BgmPath)) return;
            Engine.GetService<AudioManager>()?.ReleaseAudioResources(this, BgmPath);

            if (string.IsNullOrWhiteSpace(IntroBgmPath)) return;
            Engine.GetService<AudioManager>()?.ReleaseAudioResources(this, IntroBgmPath);
        }

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<AudioManager>();
            var allBgmState = manager.CloneAllPlayingBgmState();

            undoData.Executed = true;
            undoData.BgmState = allBgmState;

            if (string.IsNullOrWhiteSpace(BgmPath))
                await Task.WhenAll(allBgmState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, Volume, Loop, Duration, IntroBgmPath)));
            else await PlayOrModifyTrackAsync(manager, BgmPath, Volume, Loop, Duration, IntroBgmPath);
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var manager = Engine.GetService<AudioManager>();
            await manager.StopAllBgmAsync();
            await Task.WhenAll(undoData.BgmState.Select(s => PlayOrModifyTrackAsync(manager, s.Path, s.Volume, s.IsLooped, 0, null)));

            undoData = default;
        }

        private static async Task PlayOrModifyTrackAsync (AudioManager mngr, string path, float volume, bool loop, float time, string introPath)
        {
            if (mngr.IsBgmPlaying(path)) mngr.ModifyBgm(path, volume, loop, time);
            else await mngr.PlayBgmAsync(path, volume, time, loop, introPath);
        }
    } 
}
