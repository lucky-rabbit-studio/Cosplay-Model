// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.Audio;

namespace Naninovel
{
    [System.Serializable]
    public class AudioConfiguration : Configuration
    {
        public const string DefaultAudioPathPrefix = "Audio";
        public const string DefaultVoicePathPrefix = "Voice";

        [Tooltip("Configuration of the resource loader used with audio (BGM and SFX) resources.")]
        public ResourceLoaderConfiguration AudioLoader = new ResourceLoaderConfiguration { PathPrefix = DefaultAudioPathPrefix };
        [Tooltip("Configuration of the resource loader used with voice resources.")]
        public ResourceLoaderConfiguration VoiceLoader = new ResourceLoaderConfiguration { PathPrefix = DefaultVoicePathPrefix };
        [Tooltip("When enabled, each `" + nameof(Actions.PrintText) + "` action will attempt to play voice clip at `VoiceResourcesPrefix/ScriptName/LineIndex.ActionIndex`.")]
        public bool EnableAutoVoicing = false;

        [Header("Audio Mixer")]
        [Tooltip("Audio mixer to control audio groups. When not provided, will use a default one.")]
        public AudioMixer CustomAudioMixer = default;
        [Tooltip("Name of the mixer's handle to control master volume.")]
        public string MasterVolumeHandleName = "Master Volume";
        [Tooltip("Path of the mixer's group to control master volume.")]
        public string BgmGroupPath = "Master/BGM";
        [Tooltip("Name of the mixer's handle to control background music volume.")]
        public string BgmVolumeHandleName = "BGM Volume";
        [Tooltip("Path of the mixer's group to control background music volume.")]
        public string SfxGroupPath = "Master/SFX";
        [Tooltip("Name of the mixer's handle to control sound effects volume.")]
        public string SfxVolumeHandleName = "SFX Volume";
        [Tooltip("Path of the mixer's group to control sound effects volume.")]
        public string VoiceGroupPath = "Master/Voice";
        [Tooltip("Name of the mixer's handle to control voice volume.")]
        public string VoiceVolumeHandleName = "Voice Volume";
    }
}
