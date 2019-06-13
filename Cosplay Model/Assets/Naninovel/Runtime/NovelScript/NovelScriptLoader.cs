// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Loader used to load <see cref="NovelScript"/> resources.
    /// </summary>
    /// <remarks>
    /// Novel scripts are serialized (and referenced by the resource providers) as <see cref="TextAsset"/> objects (text files);
    /// we use this wrapper to automatically create <see cref="NovelScript"/> based on the loaded text resources.
    /// </remarks>
    public class NovelScriptLoader : LocalizableResourceLoader<TextAsset>
    {
        protected Dictionary<string, NovelScript> LoadedScripts { get; } = new Dictionary<string, NovelScript>();

        public NovelScriptLoader (List<IResourceProvider> providersList, LocalizationManager localizationManager, string prefix = null)
            : base(providersList, localizationManager, prefix) { }

        public NovelScriptLoader (ResourceLoaderConfiguration loaderConfig, ResourceProviderManager providerManager, LocalizationManager localeManager)
            : base(loaderConfig, providerManager, localeManager) { }

        public override bool IsLoaded (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return LoadedScripts.ContainsKey(path);
        }

        public new NovelScript GetLoadedResourceOrNull (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            LoadedScripts.TryGetValue(path, out var result);
            return result;
        }

        public new async Task<NovelScript> LoadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            var scriptName = path.Contains("/") ? path.GetAfter("/") : path;

            if (LocalizationManager is null || !await LocalizationManager.IsLocalizedResourceAvailableAsync<TextAsset>(path))
            {
                var textResource = await base.LoadAsync(path, true);
                if (textResource is null || !textResource.IsValid || textResource.Object.text is null)
                {
                    Debug.LogError($"Failed to load `{path}` novel script.");
                    return null;
                }
                var novelScript = new NovelScript(scriptName, textResource.Object.text);
                LoadedScripts[path] = novelScript;
                return novelScript;
            }

            var sourceTextResource = await Providers.LoadResourceAsync<TextAsset>(path);
            if (sourceTextResource is null || !sourceTextResource.IsValid || sourceTextResource.Object.text is null)
            {
                Debug.LogError($"Failed to load source text of the `{path}` novel script.");
                return null;
            }
            LoadedResourcePaths.Add(path);

            var localizationTextResource = await base.LoadAsync(path, true);
            if (localizationTextResource is null || !localizationTextResource.IsValid || localizationTextResource.Object.text is null)
            {
                Debug.LogError($"Failed to load localization text of the `{path}` novel script.");
                return null;
            }

            var sourceScript = new NovelScript(scriptName, sourceTextResource.Object.text);
            var localizationScript = new NovelScript($"{scriptName}-{LocalizationManager.SelectedLocale}", localizationTextResource.Object.text);
            NovelScriptLocalization.LocalizeScript(sourceScript, localizationScript);
            LoadedScripts[path] = sourceScript;
            return sourceScript;
        }

        public new async Task<IEnumerable<NovelScript>> LoadAllAsync (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            // 1. Locate all source scripts.
            var locatedResources = await Providers.LocateResourcesAsync<TextAsset>(path);
            // 2. Load localized scripts (when available).
            return await Task.WhenAll(locatedResources.Select(r => LoadAsync(r.Path, true)));
        }

        public override void Unload (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            LoadedScripts.Remove(path);
            base.Unload(path, true);
        }

        public override async Task UnloadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            LoadedScripts.Remove(path);
            await base.UnloadAsync(path, true);
        }

        public override void UnloadAll ()
        {
            LoadedScripts.Clear();
            base.UnloadAll();
        }

        public override async Task UnloadAllAsync ()
        {
            LoadedScripts.Clear();
            await base.UnloadAllAsync();
        }
    }
}
