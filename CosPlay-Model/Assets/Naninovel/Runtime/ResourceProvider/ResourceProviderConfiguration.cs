// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class ResourceProviderConfiguration : Configuration
    {
        [Tooltip("Dictates when the resources are loaded and unloaded during script execution:\n • Static — All the resources required for the script execution are pre-loaded when starting the playback and unloaded only when the script has finished playing.\n • Dynamic — Only the resources required for the next `DynamicPolicySteps` commands are pre-loaded during the script execution and all the unused resources are unloaded immediately. Use this mode when targetting platforms with strict memory limitations and it's impossible to properly orginize naninovel scripts.")]
        public ResourcePolicy ResourcePolicy = ResourcePolicy.Static;
        [Tooltip("When dynamic resource policy is enabled, defines the number of script commands to pre-load.")]
        public int DynamicPolicySteps = 25;
        [Tooltip("When dynamic resource policy is enabled, this will set Unity's background loading thread priority to low to prevent hiccups when loading resources during script playback.")]
        public bool OptimizeLoadingPriority = true;
        [Tooltip("Whether to log resource loading operations on the loading screen.")]
        public bool LogResourceLoading = false;

        [Header("Local Provider")]
        [Tooltip("Path root to use for the local resource provider.")]
        public string LocalRootPath = "Resources";

        #if UNITY_GOOGLE_DRIVE_AVAILABLE
        [Header("Google Drive Provider")]
        [Tooltip("Path root to use for the Google Drive resource provider.")]
        public string GoogleDriveRootPath = "Resources";
        [Tooltip("Maximum allowed concurrent requests when contacting Google Drive API.")]
        public int GoogleDriveRequestLimit = 2;
        [Tooltip("Cache policy to use when downloading resources. `Smart` will attempt to use Changes API to check for the modifications on the drive. `PurgeAllOnInit` will to re-download all the resources when the provider is initialized.")]
        public UnityCommon.GoogleDriveResourceProvider.CachingPolicyType GoogleDriveCachingPolicy = UnityCommon.GoogleDriveResourceProvider.CachingPolicyType.Smart;
        #endif
    }
}
