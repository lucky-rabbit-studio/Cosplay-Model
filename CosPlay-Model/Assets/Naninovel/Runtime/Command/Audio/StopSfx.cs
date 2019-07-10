// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Stops playing an SFX (sound effect) track with the provided name.
    /// </summary>
    /// <remarks>
    /// When sound effect track name (SfxPath) is not specified, will stop all the currently played tracks.
    /// </remarks>
    /// <example>
    /// ; Stop playing an SFX with the name `Rain`, fading-out for 15 seconds.
    /// @stopSfx Rain fadeTime:15
    /// 
    /// ; Stops all the currently played sound effect tracks
    /// @stopSfx
    /// </example>
    public class StopSfx : Command
    {
        private struct UndoData { public bool Executed; public List<AudioManager.ClipState> SfxState; }

        /// <summary>
        /// Path to the sound effect to stop.
        /// </summary>
        [CommandParameter(NamelessParameterAlias, true)]
        public string SfxPath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<AudioManager>();
            var allSfxState = manager.CloneAllPlayingSfxState();

            undoData.Executed = true;
            undoData.SfxState = allSfxState;

            if (string.IsNullOrWhiteSpace(SfxPath))
                await manager.StopAllSfxAsync(Duration);
            else await manager.StopSfxAsync(SfxPath, Duration);
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var manager = Engine.GetService<AudioManager>();
            await manager.StopAllSfxAsync();
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
