// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class ScriptsConfiguration : Configuration
    {
        public const string DefaultScriptsPathPrefix = "Scripts";

        [Tooltip("Configuration of the resource loader used with naninovel script resources.")]
        public ResourceLoaderConfiguration Loader = new ResourceLoaderConfiguration { PathPrefix = DefaultScriptsPathPrefix };
        [Tooltip("Name of the script which contains global define expressions, that should be accessible from all the other scripts.")]
        public string GlobalDefinesScript = default;
        [Tooltip("Name of the script to play right after the engine initialization.")]
        public string InitializationScript = default;
        [Tooltip("Name of the script to play when showing the Title UI. Can be used to setup the title screen scene (backgound, music, etc).")]
        public string TitleScript = default;
        [Tooltip("Name of the script to play when starting a new game. Will use first available when not provided.")]
        public string StartGameScript = default;

        [Header("Community Modding")]
        [Tooltip("Whether to allow adding external naninovel scripts to the build.")]
        public bool EnableCommunityModding = false;
        [Tooltip("Configuration of the resource loader used with external naninovel script resources.")]
        public ResourceLoaderConfiguration ExternalLoader = new ResourceLoaderConfiguration {
            ProviderTypes = new List<ResourceProviderType> { ResourceProviderType.Local },
            PathPrefix = DefaultScriptsPathPrefix
        };

        [Header("Script Navigator")]
        [Tooltip("Whether to initializte script navigator to browse available naninovel scripts.")]
        public bool EnableNavigator = true;
        [Tooltip("Whether to show naninovel script navigator when script manager is initialized.")]
        public bool ShowNavigatorOnInit = false;
        [Tooltip("UI sort order of the script navigator.")]
        public int NavigatorSortOrder = 900;
    }
}
