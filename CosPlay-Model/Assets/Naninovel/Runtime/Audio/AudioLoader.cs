// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public class AudioLoader : LocalizableResourceLoader<AudioClip>
    {
        public AudioLoader (List<IResourceProvider> providersList, LocalizationManager localizationManager, string prefix = null) 
            : base(providersList, localizationManager, prefix) { }

        public AudioLoader (ResourceLoaderConfiguration loaderConfig, ResourceProviderManager providerManager, LocalizationManager localeManager)
            : base(loaderConfig, providerManager, localeManager) { }

        public override async Task<Resource<AudioClip>> LoadAsync (string path, bool isFullPath = false)
        {
            var resource = await base.LoadAsync(path, isFullPath);
            if (resource != null && resource.IsValid)
                resource.Object.name = NameFromPath(path);
            return resource;
        }

        private string NameFromPath (string path) => path.Contains("/") ? path.GetAfter("/") : path;
    }
}
