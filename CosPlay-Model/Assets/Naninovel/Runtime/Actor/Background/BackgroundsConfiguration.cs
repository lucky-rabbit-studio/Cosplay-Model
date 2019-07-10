// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class BackgroundsConfiguration : OrthoActorManagerConfiguration
    {
        public const string DefaultBackgroundsPathPrefix = "Backgrounds";

        [Tooltip("Z-axis offset distance (depth) from background actors to the camera.")]
        public int ZOffset = 100;
        [Tooltip("Metadata to use by default when creating background actors and custom metadata for the created actor ID doesn't exist.")]
        public BackgroundMetadata DefaultMetadata = new BackgroundMetadata();
        [Tooltip("Metadata to use when creating background actors with specific IDs.")]
        public BackgroundMetadata.Map Metadata = new BackgroundMetadata.Map {
            [BackgroundManager.MainActorId] = new BackgroundMetadata()
        };
    }
}
