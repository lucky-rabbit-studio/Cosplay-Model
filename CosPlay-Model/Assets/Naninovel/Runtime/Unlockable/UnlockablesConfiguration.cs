// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class UnlockablesConfiguration : Configuration
    {
        public const string DefaultUnlockablesPathPrefix = "Unlockables";

        [Tooltip("Configuration of the resource loader used with unlockable resources.")]
        public ResourceLoaderConfiguration LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = DefaultUnlockablesPathPrefix };
    }
}
