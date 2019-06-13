// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Allows working with localizable resources.
    /// </summary>
    public class LocalizableResourceLoader<TResource> : ResourceLoader<TResource> where TResource : UnityEngine.Object
    {
        protected LocalizationManager LocalizationManager { get; }
        protected HashSet<string> LoadedLocalizedResourcePaths { get; }

        public LocalizableResourceLoader (List<IResourceProvider> providersList, LocalizationManager localizationManager, string prefix = null)
            : base(providersList, prefix)
        {
            LocalizationManager = localizationManager;
            LoadedLocalizedResourcePaths = new HashSet<string>();
        }

        public LocalizableResourceLoader (ResourceLoaderConfiguration loaderConfig, ResourceProviderManager providerManager, LocalizationManager localeManager)
            : this(providerManager.GetProviderList(loaderConfig.ProviderTypes), localeManager, loaderConfig.PathPrefix) { }

        public override bool IsLoaded (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            if (LocalizationManager != null && LocalizationManager.IsLocalizedResourceLoaded(path)) return true;

            // We should also check if a localizable version exists before falling back to the base implementation,
            // but network providers doesn't support blocking API, so leaving it as is for now.
            return base.IsLoaded(path, true);
        }

        public override Resource<TResource> GetLoadedResourceOrNull (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            return LocalizationManager?.GetLoadedLocalizedResourceOrNull<TResource>(path) ?? base.GetLoadedResourceOrNull(path, true);
        }

        public override Resource<TResource> Load (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            if (LocalizationManager is null || !LocalizationManager.IsLocalizedResourceAvailable<TResource>(path))
                return base.Load(path, true);

            var localizedResource = LocalizationManager.LoadLocalizedResource<TResource>(path);
            if (localizedResource != null && localizedResource.IsValid)
                LoadedLocalizedResourcePaths.Add(localizedResource.Path);
            return localizedResource;
        }

        public override async Task<Resource<TResource>> LoadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            if (LocalizationManager is null || !await LocalizationManager.IsLocalizedResourceAvailableAsync<TResource>(path))
                return await base.LoadAsync(path, true);

            var localizedResource = await LocalizationManager.LoadLocalizedResourceAsync<TResource>(path);
            if (localizedResource != null && localizedResource.IsValid)
                LoadedLocalizedResourcePaths.Add(localizedResource.Path);
            return localizedResource;
        }

        public override IEnumerable<Resource<TResource>> LoadAll (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            if (LocalizationManager is null)
                return base.LoadAll(path, true);

            // 1. Locate all the original resources.
            var locatedResources = base.LocateResources(path, true);
            // 2. Load localized resources when available, original otherwise.
            return locatedResources.Select(r => Load(r.Path, true));
        }

        public override async Task<IEnumerable<Resource<TResource>>> LoadAllAsync (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            if (LocalizationManager is null)
                return await base.LoadAllAsync(path, true);

            // 1. Locate all the original resources.
            var locatedResources = await base.LocateResourcesAsync(path, true);
            // 2. Load localized resources when available, original otherwise.
            return await Task.WhenAll(locatedResources.Select(r => LoadAsync(r.Path, true)));
        }

        public override void Unload (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            LocalizationManager?.UnloadLocalizedResource(path);
            LoadedLocalizedResourcePaths.Remove(path);

            base.Unload(path, true);
        }

        public override async Task UnloadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            await LocalizationManager?.UnloadLocalizedResourceAsync(path);
            LoadedLocalizedResourcePaths.Remove(path);

            await base.UnloadAsync(path, true);
        }

        /// <summary>
        /// Unloads all the resources (both localized and originals) previously loaded by this loader.
        /// </summary>
        public override void UnloadAll ()
        {
            base.UnloadAll();

            foreach (var path in LoadedLocalizedResourcePaths)
                Unload(path, true);
        }

        /// <summary>
        /// Asynchronously unloads all the resources (both localized and originals) previously loaded by this loader.
        /// </summary>
        public override async Task UnloadAllAsync ()
        {
            await base.UnloadAllAsync();

            // To prevent modifying the collection on iteration.
            var paths = LoadedLocalizedResourcePaths.ToList();
            foreach (var path in paths)
                await UnloadAsync(path, true);
        }
    }
}
