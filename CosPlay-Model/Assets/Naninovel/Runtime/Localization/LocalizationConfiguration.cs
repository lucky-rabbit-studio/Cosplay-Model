// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class LocalizationConfiguration : Configuration
    {
        public const string DefaultLocalizationPathPrefix = "Localization";

        [Tooltip("Configuration of the resource loader used with the localization resources.")]
        public ResourceLoaderConfiguration LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = DefaultLocalizationPathPrefix };
        [Tooltip("Default locale of the game. When user selects a default locale, original resources will be used.")]
        public string DefaultLocale = "en";
    }
}
