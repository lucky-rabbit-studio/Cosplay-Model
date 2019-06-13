// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class ResourceProviderConfiguration : Configuration
    {
        [Tooltip("Whether to log resource loading operations on the loading screen.")]
        public bool LogResourceLoading = true;

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
