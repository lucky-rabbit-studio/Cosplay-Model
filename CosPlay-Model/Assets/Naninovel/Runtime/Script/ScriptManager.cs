// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages <see cref="Script"/> resources.
    /// </summary>
    [InitializeAtRuntime]
    public class ScriptManager : IEngineService
    {
        public event Action OnScriptLoadStarted;
        public event Action OnScriptLoadCompleted;

        public string StartGameScriptName { get; private set; }
        public bool CommunityModdingEnabled => config.EnableCommunityModding;
        public bool IsNavigatorAvailable => navigatorUI;
        public bool IsNavigatorVisible => IsNavigatorAvailable && navigatorUI.IsVisible;
        public bool IsLoadingScripts { get; private set; }

        private const string navigatorPrefabResourcesPath = "Naninovel/ScriptNavigator";

        private readonly ScriptsConfiguration config;
        private ScriptLoader scriptLoader;
        private ScriptLoader externalScriptLoader;
        private ResourceProviderManager providersManager;
        private LocalizationManager localizationManager;
        private UI.ScriptNavigatorPanel navigatorUI;

        public ScriptManager (ScriptsConfiguration config, ResourceProviderManager providersManager, LocalizationManager localizationManager)
        {
            this.config = config;
            this.providersManager = providersManager;
            this.localizationManager = localizationManager;
        }

        public async Task InitializeServiceAsync ()
        {
            scriptLoader = new ScriptLoader(config.Loader, providersManager, localizationManager);

            if (!string.IsNullOrEmpty(config.GlobalDefinesScript))
                scriptLoader.GlobalDefinesScript = await scriptLoader.LoadAsync(config.GlobalDefinesScript);

            if (CommunityModdingEnabled)
            {
                externalScriptLoader = new ScriptLoader(config.ExternalLoader, providersManager, localizationManager);
                externalScriptLoader.GlobalDefinesScript = scriptLoader.GlobalDefinesScript;
            }

            if (Application.isPlaying && config.EnableNavigator)
            {
                var navigatorPrefab = Resources.Load<UI.ScriptNavigatorPanel>(navigatorPrefabResourcesPath);
                navigatorUI = Engine.Instantiate(navigatorPrefab, "ScriptNavigator");
                navigatorUI.SortingOrder = config.NavigatorSortOrder;
                navigatorUI.SetIsVisible(false);
                if (config.ShowNavigatorOnInit) ShowNavigator();
            }

            if (string.IsNullOrEmpty(config.StartGameScript))
            {
                var scriptPaths = await scriptLoader.LocateAsync(string.Empty);
                StartGameScriptName = scriptPaths.FirstOrDefault()?.Replace(scriptLoader.PathPrefix + "/", string.Empty);
            }
            else StartGameScriptName = config.StartGameScript;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            if (navigatorUI)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(navigatorUI);
                else UnityEngine.Object.DestroyImmediate(navigatorUI);
            }
        }

        public async Task<IEnumerable<Script>> LoadExternalScriptsAsync ()
        {
            if (!CommunityModdingEnabled)
                return new List<Script>();

            InvokeOnScriptLoadStarted();
            var scripts = await externalScriptLoader.LoadAllAsync();
            InvokeOnScriptLoadCompleted();
            return scripts;
        }

        public async Task<Script> LoadScriptAsync (string name)
        {
            InvokeOnScriptLoadStarted();

            if (scriptLoader.IsLoaded(name))
            {
                InvokeOnScriptLoadCompleted();
                return scriptLoader.GetLoadedOrNull(name);
            }

            var script = await scriptLoader.LoadAsync(name);
            InvokeOnScriptLoadCompleted();
            return script;
        }

        public async Task<IEnumerable<Script>> LoadAllScriptsAsync ()
        {
            InvokeOnScriptLoadStarted();
            var scripts = await scriptLoader.LoadAllAsync();
            GenerateNavigatorButtons(scripts);
            InvokeOnScriptLoadCompleted();
            return scripts;
        }

        public void UnloadAllScripts ()
        {
            scriptLoader.UnloadAll();

            #if UNITY_GOOGLE_DRIVE_AVAILABLE
            // Delete cached scripts when using Google Drive resource provider.
            if (providersManager.IsProviderInitialized(ResourceProviderType.GoogleDrive))
                (providersManager.GetProvider(ResourceProviderType.GoogleDrive) as UnityCommon.GoogleDriveResourceProvider).PurgeCachedResources(config.Loader.PathPrefix);
            #endif

            if (IsNavigatorAvailable) navigatorUI.DestroyScriptButtons();
        }

        public async Task ReloadAllScriptsAsync ()
        {
            UnloadAllScripts();
            await LoadAllScriptsAsync();
        }

        public void ShowNavigator () => navigatorUI?.Show();
        public void HideNavigator () => navigatorUI?.Hide();
        public void ToggleNavigator () => navigatorUI?.ToggleVisibility();

        private void GenerateNavigatorButtons (IEnumerable<Script> scripts) => navigatorUI?.GenerateScriptButtons(scripts);

        private void InvokeOnScriptLoadStarted ()
        {
            IsLoadingScripts = true;
            OnScriptLoadStarted?.Invoke();
        }

        private void InvokeOnScriptLoadCompleted ()
        {
            IsLoadingScripts = false;
            OnScriptLoadCompleted?.Invoke();
        }
    }
}
