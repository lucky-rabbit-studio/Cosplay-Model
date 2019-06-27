// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Loader used to load <see cref="Script"/> resources.
    /// </summary>
    /// <remarks>
    /// Naninovel scripts are serialized (and referenced by the resource providers) as <see cref="ScriptAsset"/> objects (imported over text files);
    /// we use this wrapper to automatically create <see cref="Script"/> based on the loaded script resources.
    /// </remarks>
    public class ScriptLoader : LocalizableResourceLoader<ScriptAsset>
    {
        public Script GlobalDefinesScript { get; set; }

        protected readonly Dictionary<string, Script> LoadedScripts = new Dictionary<string, Script>();

        public ScriptLoader (List<IResourceProvider> providersList, LocalizationManager localizationManager, string prefix = null)
            : base(providersList, localizationManager, prefix) { }

        public ScriptLoader (ResourceLoaderConfiguration loaderConfig, ResourceProviderManager providerManager, LocalizationManager localeManager)
            : base(loaderConfig, providerManager, localeManager) { }

        public override bool IsLoaded (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return LoadedScripts.ContainsKey(path);
        }

        public new Script GetLoadedOrNull (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            LoadedScripts.TryGetValue(path, out var result);
            return result;
        }

        public new async Task<Script> LoadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            var scriptName = path.Contains("/") ? path.GetAfter("/") : path;

            if (LocalizationManager is null || !await LocalizationManager.IsLocalizedResourceAvailableAsync<ScriptAsset>(path))
            {
                var textResource = await base.LoadAsync(path, true);
                if (textResource is null || !textResource.IsValid || textResource.Object.ScriptText is null)
                {
                    Debug.LogError($"Failed to load `{path}` naninovel script.");
                    return null;
                }
                var script = new Script(scriptName, textResource.Object.ScriptText, GlobalDefinesScript?.DefineLines);
                LoadedScripts[path] = script;
                return script;
            }

            var sourceTextResource = await Providers.LoadResourceAsync<ScriptAsset>(path);
            if (sourceTextResource is null || !sourceTextResource.IsValid || sourceTextResource.Object.ScriptText is null)
            {
                Debug.LogError($"Failed to load source text of the `{path}` naninovel script.");
                return null;
            }
            LoadedResources.Add(sourceTextResource);

            var localizationTextResource = await base.LoadAsync(path, true);
            if (localizationTextResource is null || !localizationTextResource.IsValid || localizationTextResource.Object.ScriptText is null)
            {
                Debug.LogError($"Failed to load localization text of the `{path}` naninovel script.");
                return null;
            }

            var sourceScript = new Script(scriptName, sourceTextResource.Object.ScriptText, GlobalDefinesScript?.DefineLines);
            var localizationScript = new Script($"{scriptName}-{LocalizationManager.SelectedLocale}", localizationTextResource.Object.ScriptText, GlobalDefinesScript?.DefineLines);
            ScriptLocalization.LocalizeScript(sourceScript, localizationScript);
            LoadedScripts[path] = sourceScript;
            return sourceScript;
        }

        public new async Task<IEnumerable<Script>> LoadAllAsync (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            // 1. Locate all source scripts.
            var locatedResourcePaths = await Providers.LocateResourcesAsync<ScriptAsset>(path);
            // 2. Load localized scripts (when available).
            return await Task.WhenAll(locatedResourcePaths.Select(p => LoadAsync(p, true)));
        }

        public override void Unload (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            LoadedScripts.Remove(path);
            base.Unload(path, true);
        }

        public override void UnloadAll ()
        {
            LoadedScripts.Clear();
            base.UnloadAll();
        }
    }
}
