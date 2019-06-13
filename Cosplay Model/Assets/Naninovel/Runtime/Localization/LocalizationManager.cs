// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages the localization activities.
    /// </summary>
    [InitializeAtRuntime]
    public class LocalizationManager : IStatefulService<SettingsStateMap>
    {
        [Serializable]
        private class Settings
        {
            public string SelectedLocale;
        }

        /// <summary>
        /// Whether the game is currently running in the default locale.
        /// </summary>
        public bool UsingDefaulLocale => SelectedLocale == DefaultLocale;
        public string DefaultLocale => config.DefaultLocale;
        public string SelectedLocale { get; private set; }
        public List<string> AvailableLocales { get; private set; }

        private readonly LocalizationConfiguration config;
        private ResourceProviderManager providersManager;
        private List<IResourceProvider> providerList;
        private HashSet<Func<Task>> changeLocaleCallbacks;

        public LocalizationManager (LocalizationConfiguration config, ResourceProviderManager providersManager)
        {
            AvailableLocales = new List<string>();
            changeLocaleCallbacks = new HashSet<Func<Task>>();
            this.config = config;
            this.providersManager = providersManager;
        }

        public async Task InitializeServiceAsync ()
        {
            providerList = providersManager.GetProviderList(config.LoaderConfiguration.ProviderTypes);
            await RetrieveAvailableLocalesAsync();
        }

        public void ResetService () { }

        public void DestroyService () { }

        public Task SaveServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = new Settings() {
                SelectedLocale = SelectedLocale
            };
            stateMap.SerializeObject(settings);
            return Task.CompletedTask;
        }

        public async Task LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.DeserializeObject<Settings>() ?? new Settings { SelectedLocale = DefaultLocale };
            await SelectLocaleAsync(settings.SelectedLocale ?? DefaultLocale);
        }

        public bool IsLocaleAvailable (string locale) => AvailableLocales.Contains(locale);

        public async Task SelectLocaleAsync (string locale)
        {
            if (!IsLocaleAvailable(locale))
            {
                Debug.LogWarning($"Failed to select locale: Locale `{locale}` is not available.");
                return;
            }

            if (locale == SelectedLocale) return;
            SelectedLocale = locale;
            if (changeLocaleCallbacks.Count > 0) await Task.WhenAll(changeLocaleCallbacks.Select(c => c.Invoke()));
        }

        public void AddChangeLocaleCallback (Func<Task> callback) => changeLocaleCallbacks.Add(callback);

        public void RemoveChangeLocaleCallback (Func<Task> callback) => changeLocaleCallbacks.Remove(callback);

        public bool IsLocalizedResourceAvailable<TResource> (string path) where TResource : UnityEngine.Object
        {
            if (UsingDefaulLocale) return false;
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return providerList.ResourceExists<TResource>(localizedResourcePath);
        }

        public async Task<bool> IsLocalizedResourceAvailableAsync<TResource> (string path) where TResource : UnityEngine.Object
        {
            if (UsingDefaulLocale) return false;
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return await providerList.ResourceExistsAsync<TResource>(localizedResourcePath);
        }

        public Resource<TResource> LoadLocalizedResource<TResource> (string path) where TResource : UnityEngine.Object
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return providerList.LoadResource<TResource>(localizedResourcePath);
        }

        public async Task<Resource<TResource>> LoadLocalizedResourceAsync<TResource> (string path) where TResource : UnityEngine.Object
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return await providerList.LoadResourceAsync<TResource>(localizedResourcePath);
        }

        public Resource<TResource> GetLoadedLocalizedResourceOrNull<TResource> (string path) where TResource : UnityEngine.Object
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return providerList.GetLoadedResourceOrNull<TResource>(path);
        }

        public void UnloadLocalizedResource (string path)
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            providerList.UnloadResource(localizedResourcePath);
        }

        public async Task UnloadLocalizedResourceAsync (string path)
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            await providerList.UnloadResourceAsync(localizedResourcePath);
        }

        public bool IsLocalizedResourceLoaded (string path)
        {
            var localizedResourcePath = BuildLocalizedResourcePath(path);
            return providerList.ResourceLoaded(localizedResourcePath);
        }

        /// <summary>
        /// Retrieves available localizations by locating folders inside the localization resources root.
        /// Folder names should correspond to the <see cref="LanguageTags"/> tag entries (RFC5646).
        /// </summary>
        private async Task RetrieveAvailableLocalesAsync ()
        {
            var resources = await providerList.LocateFoldersAsync(config.LoaderConfiguration.PathPrefix);
            AvailableLocales = resources.Select(r => r.Name).Where(tag => LanguageTags.ContainsTag(tag)).ToList();
            AvailableLocales.Add(DefaultLocale);
        }

        private string BuildLocalizedResourcePath (string resourcePath) => $"{config.LoaderConfiguration.PathPrefix}/{SelectedLocale}/{resourcePath}";
    }
}
