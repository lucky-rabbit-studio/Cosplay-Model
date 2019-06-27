// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    /// <summary>
    /// Manages the audio: SFX, BGM and voice.
    /// </summary>
    [InitializeAtRuntime]
    public class AudioManager : IStatefulService<SettingsStateMap>, IStatefulService<GameStateMap>
    {
        [System.Serializable]
        public class ClipState { public string Path; public float Volume; public bool IsLooped; }

        [System.Serializable]
        private class Settings
        {
            public float MasterVolume = 1f;
            public float BgmVolume = 1f;
            public float SfxVolume = 1f;
            public float VoiceVolume = 1f;
        }

        [System.Serializable]
        private class GameState { public List<ClipState> BgmClips; public List<ClipState> SfxClips; }

        public bool AutoVoicingEnabled => config.EnableAutoVoicing;
        public float MasterVolume { get => GetMixerVolume(config.MasterVolumeHandleName); set => SetMixerVolume(config.MasterVolumeHandleName, value); }
        public float BgmVolume { get => GetMixerVolume(config.BgmVolumeHandleName); set { if (BgmGroupAvailable) SetMixerVolume(config.BgmVolumeHandleName, value); } }
        public float SfxVolume { get => GetMixerVolume(config.SfxVolumeHandleName); set { if (SfxGroupAvailable) SetMixerVolume(config.SfxVolumeHandleName, value); } }
        public float VoiceVolume { get => GetMixerVolume(config.VoiceVolumeHandleName); set { if (VoiceGroupAvailable) SetMixerVolume(config.VoiceVolumeHandleName, value); } }
        public bool BgmGroupAvailable => bgmGroup;
        public bool SfxGroupAvailable => sfxGroup;
        public bool VoiceGroupAvailable => voiceGroup;

        protected AudioMixer AudioMixer { get; private set; }

        private const string defaultMixerResourcesPath = "Naninovel/DefaultMixer";

        private readonly AudioConfiguration config;
        private ResourceProviderManager providersManager;
        private LocalizationManager localizationManager;
        private AudioLoader audioLoader, voiceLoader;
        private AudioController audioController;
        private AudioMixerGroup bgmGroup, sfxGroup, voiceGroup;
        private ClipState voiceClip;
        private List<ClipState> bgmClips, sfxClips;

        public AudioManager (AudioConfiguration config, ResourceProviderManager providersManager, LocalizationManager localizationManager)
        {
            this.config = config;
            this.providersManager = providersManager;
            this.localizationManager = localizationManager;

            AudioMixer = ObjectUtils.IsValid(config.CustomAudioMixer) ? config.CustomAudioMixer : Resources.Load<AudioMixer>(defaultMixerResourcesPath);

            if (ObjectUtils.IsValid(AudioMixer))
            {
                bgmGroup = AudioMixer.FindMatchingGroups(config.BgmGroupPath)?.FirstOrDefault();
                sfxGroup = AudioMixer.FindMatchingGroups(config.SfxGroupPath)?.FirstOrDefault();
                voiceGroup = AudioMixer.FindMatchingGroups(config.VoiceGroupPath)?.FirstOrDefault();
            }

            bgmClips = new List<ClipState>();
            sfxClips = new List<ClipState>();
        }

        public Task InitializeServiceAsync ()
        {
            audioLoader = new AudioLoader(config.AudioLoader, providersManager, localizationManager);
            voiceLoader = new AudioLoader(config.VoiceLoader, providersManager, localizationManager);
            audioController = Engine.CreateObject<AudioController>();

            return Task.CompletedTask;
        }

        public void ResetService ()
        {
            audioController.StopAllClips();
            bgmClips.Clear();
            sfxClips.Clear();
            voiceClip = null;

            audioLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
            voiceLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
        }

        public void DestroyService ()
        {
            if (audioController)
            {
                audioController.StopAllClips();
                Object.Destroy(audioController.gameObject);
            }

            audioLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
            voiceLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
        }

        public Task SaveServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                MasterVolume = MasterVolume,
                BgmVolume = BgmVolume,
                SfxVolume = SfxVolume,
                VoiceVolume = VoiceVolume
            };
            stateMap.SerializeObject(settings);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.DeserializeObject<Settings>() ?? new Settings();
            MasterVolume = settings.MasterVolume;
            BgmVolume = settings.BgmVolume;
            SfxVolume = settings.SfxVolume;
            VoiceVolume = settings.VoiceVolume;
            return Task.CompletedTask;
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var state = new GameState() {
                BgmClips = CloneAllPlayingBgmState(),
                SfxClips = CloneAllPlayingSfxState()
            };
            stateMap.SerializeObject(state);
            return Task.CompletedTask;
        }

        public async Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            var tasks = new List<Task>();
            if (state.BgmClips != null)
                foreach (var clipState in state.BgmClips)
                    tasks.Add(PlayBgmAsync(clipState.Path, clipState.Volume, 0, clipState.IsLooped));
            if (state.SfxClips != null)
                foreach (var clipState in state.SfxClips)
                    tasks.Add(PlaySfxAsync(clipState.Path, clipState.Volume, 0, clipState.IsLooped));
            await Task.WhenAll(tasks);
        }

        public async Task HoldAudioResourcesAsync (object holder, string path)
        {
            var resource = await audioLoader.LoadAsync(path);
            if (resource.IsValid)
                resource.Hold(holder);
        }

        public void ReleaseAudioResources (object holder, string path)
        {
            if (!audioLoader.IsLoaded(path)) return;

            var resource = audioLoader.GetLoadedOrNull(path);
            resource.Release(holder, false);
            if (resource.HoldersCount == 0)
            {
                audioController.StopClip(resource);
                resource.Provider.UnloadResource(resource.Path);
            }
        }

        public async Task HoldVoiceResourcesAsync (object holder, string path)
        {
            var resource = await voiceLoader.LoadAsync(path);
            if (resource.IsValid)
                resource.Hold(holder);
        }

        public void ReleaseVoiceResources (object holder, string path)
        {
            if (!voiceLoader.IsLoaded(path)) return;

            var resource = voiceLoader.GetLoadedOrNull(path);
            resource.Release(holder, false);
            if (resource.HoldersCount == 0)
            {
                audioController.StopClip(resource);
                resource.Provider.UnloadResource(resource.Path);
            }
        }

        public bool IsBgmPlaying (string path)
        {
            if (!bgmClips.Exists(c => c.Path == path)) return false;
            return IsAudioPlaying(path);
        }

        public bool IsSfxPlaying (string path)
        {
            if (!sfxClips.Exists(c => c.Path == path)) return false;
            return IsAudioPlaying(path);
        }

        public bool IsVoicePlaying (string path)
        {
            if (voiceClip is null || voiceClip.Path != path) return false;
            if (!voiceLoader.IsLoaded(path)) return false;
            var clipResource = voiceLoader.GetLoadedOrNull(path);
            if (!clipResource.IsValid) return false;
            return audioController.GetTrack(clipResource)?.IsPlaying ?? false;
        }

        public async Task<bool> AudioExistsAsync (string path) => await audioLoader.ExistsAsync(path);

        public async Task<bool> VoiceExistsAsync (string path) => await voiceLoader.ExistsAsync(path);

        public void ModifyBgm (string path, float volume, bool loop, float time)
        {
            var state = bgmClips.Find(c => c.Path == path);
            if (state is null) return;
            state.Volume = volume;
            state.IsLooped = loop;
            ModifyAudio(path, volume, loop, time);
        }

        public void ModifySfx (string path, float volume, bool loop, float time)
        {
            var state = sfxClips.Find(c => c.Path == path);
            if (state is null) return;
            state.Volume = volume;
            state.IsLooped = loop;
            ModifyAudio(path, volume, loop, time);
        }

        /// <summary>
        /// Will play an SFX with the provided name if it's already loaded and won't save the state.
        /// </summary>
        public void PlaySfxFast (string path, float volume = 1f, bool restartIfPlaying = true)
        {
            if (!audioLoader.IsLoaded(path)) return;
            var clip = audioLoader.GetLoadedOrNull(path);
            if (!restartIfPlaying && audioController.IsClipPlaying(clip)) return;
            audioController.PlayClip(clip, null, volume, false, sfxGroup);
        }

        public async Task PlayBgmAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = true, string introPath = null)
        {
            var stateExists = bgmClips.Exists(c => c.Path == path);
            var clipState = stateExists ? bgmClips.Find(c => c.Path == path) : new ClipState { Path = path };
            clipState.IsLooped = loop;
            clipState.Volume = volume;

            var clipResource = await audioLoader.LoadAsync(path);
            if (!clipResource.IsValid)
            {
                Debug.LogWarning($"Failed to play BGM `{path}`: resource not found.");
                return;
            }
            clipResource.Hold(this);

            var introClip = default(AudioClip);
            if (!string.IsNullOrEmpty(introPath))
            {
                var introClipResource = await audioLoader.LoadAsync(introPath);
                if (!introClipResource.IsValid)
                    Debug.LogWarning($"Failed to load intro BGM `{path}`: resource not found.");
                else
                {
                    introClipResource.Hold(this);
                    introClip = introClipResource.Object;
                }
            }

            if (!stateExists) bgmClips.Add(clipState);

            if (fadeTime <= 0) audioController.PlayClip(clipResource, null, volume, loop, bgmGroup, introClip);
            else audioController.PlayClipAsync(clipResource, fadeTime, null, volume, loop, bgmGroup, introClip).WrapAsync();
        }

        public async Task StopBgmAsync (string path, float fadeTime = 0f)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            bgmClips.RemoveAll(c => c.Path == path);

            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (fadeTime <= 0) audioController.StopClip(clipResource);
            else await audioController.StopClipAsync(clipResource, fadeTime);

            if (!IsBgmPlaying(path))
                clipResource?.Release(this);
        }

        public async Task StopAllBgmAsync (float fadeTime = 0f)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < bgmClips.Count; i++)
            {
                var state = bgmClips[i];
                tasks.Add(StopBgmAsync(state.Path, fadeTime));
            }

            await Task.WhenAll(tasks);
        }

        public async Task PlaySfxAsync (string path, float volume = 1f, float fadeTime = 0f, bool loop = false)
        {
            var stateExists = sfxClips.Exists(c => c.Path == path);
            var clipState = stateExists ? sfxClips.Find(c => c.Path == path) : new ClipState { Path = path };
            clipState.IsLooped = loop;
            clipState.Volume = volume;

            var clipResource = await audioLoader.LoadAsync(path);
            if (!clipResource.IsValid)
            {
                Debug.LogWarning($"Failed to play SFX `{path}`: resource not found.");
                return;
            }

            clipResource.Hold(this);

            if (!stateExists) sfxClips.Add(clipState);

            if (fadeTime <= 0) audioController.PlayClip(clipResource, null, volume, loop, sfxGroup);
            else audioController.PlayClipAsync(clipResource, fadeTime, null, volume, loop, sfxGroup).WrapAsync();
        }

        public async Task StopSfxAsync (string path, float fadeTime = 0f)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            sfxClips.RemoveAll(c => c.Path == path);

            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (fadeTime <= 0) audioController.StopClip(clipResource);
            else await audioController.StopClipAsync(clipResource, fadeTime);

            if (!IsSfxPlaying(path))
                clipResource?.Release(this);
        }

        public async Task StopAllSfxAsync (float fadeTime = 0f)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < sfxClips.Count; i++)
            {
                var state = sfxClips[i];
                tasks.Add(StopSfxAsync(state.Path, fadeTime));
            }

            await Task.WhenAll(tasks);
        }

        public async Task PlayVoiceAsync (string path, float volume = 1f)
        {
            StopVoice();
            voiceClip = new ClipState { Path = path, IsLooped = false, Volume = volume };
            var clipResource = await voiceLoader.LoadAsync(path);
            if (clipResource.IsValid) audioController.PlayClip(clipResource, volume: volume, mixerGroup: voiceGroup);

            clipResource.Hold(this);
        }

        public async Task PlayVoiceSequenceAsync (List<string> pathList, float volume = 1f)
        {
            foreach (var path in pathList)
            {
                await PlayVoiceAsync(path, volume);
                await new WaitWhile(() => IsVoicePlaying(path));
            }
        }

        public void StopVoice ()
        {
            if (voiceClip is null) return;

            var clipResource = voiceLoader.GetLoadedOrNull(voiceClip.Path);
            voiceClip = null;
            audioController.StopClip(clipResource);
            clipResource.Release(this);
        }

        public List<ClipState> CloneAllPlayingBgmState ()
        {
            return bgmClips.Where(c => IsAudioPlaying(c.Path)).Select(c => new ClipState { Path = c.Path, Volume = c.Volume, IsLooped = c.IsLooped }).ToList();
        }

        public List<ClipState> CloneAllPlayingSfxState ()
        {
            return sfxClips.Where(c => IsAudioPlaying(c.Path)).Select(c => new ClipState { Path = c.Path, Volume = c.Volume, IsLooped = c.IsLooped }).ToList();
        }

        private bool IsAudioPlaying (string path)
        {
            if (!audioLoader.IsLoaded(path)) return false;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (!clipResource.IsValid) return false;
            return audioController.GetTrack(clipResource)?.IsPlaying ?? false;
        }

        private void ModifyAudio (string path, float volume, bool loop, float time)
        {
            if (!audioLoader.IsLoaded(path)) return;
            var clipResource = audioLoader.GetLoadedOrNull(path);
            if (!clipResource.IsValid) return;
            var track = audioController.GetTrack(clipResource);
            if (track is null) return;
            track.IsLooped = loop;
            if (time <= 0) track.Volume = volume;
            else track.FadeAsync(volume, time).WrapAsync();
        }

        private float GetMixerVolume (string handleName)
        {
            float value;

            if (ObjectUtils.IsValid(AudioMixer))
            {
                AudioMixer.GetFloat(handleName, out value);
                value = MathUtils.DecibelToLinear(value);
            }
            else value = audioController.Volume;

            return value;
        }

        private void SetMixerVolume (string handleName, float value)
        {
            if (ObjectUtils.IsValid(AudioMixer))
                AudioMixer.SetFloat(handleName, MathUtils.LinearToDecibel(value));
            else audioController.Volume = value;
        }
    }
}
