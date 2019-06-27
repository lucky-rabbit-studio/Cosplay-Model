// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Stops playing a BGM (background music) track with the provided name.
    /// </summary>
    /// <remarks>
    /// When music track name (BgmPath) is not specified, will stop all the currently played tracks.
    /// </remarks>
    /// <example>
    /// ; Fades-out the `Promenade` music track over 10 seconds and stops the playback
    /// @stopBgm Promenade time:10
    /// 
    /// ; Stops all the currently played music tracks
    /// @stopBgm 
    /// </example>
    public class StopBgm : Command
    {
        private struct UndoData { public bool Executed; public List<AudioManager.ClipState> BgmState; }

        /// <summary>
        /// Path to the music track to stop.
        /// </summary>
        [CommandParameter(NamelessParameterAlias, true)]
        public string BgmPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<AudioManager>();
            var allBgmState = manager.CloneAllPlayingBgmState();

            undoData.Executed = true;
            undoData.BgmState = allBgmState;

            if (string.IsNullOrWhiteSpace(BgmPath))
                await manager.StopAllBgmAsync(Duration);
            else await manager.StopBgmAsync(BgmPath, Duration);
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var manager = Engine.GetService<AudioManager>();
            await manager.StopAllBgmAsync();
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
