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
    /// Manages <see cref="IResourceProvider"/> objects.
    /// </summary>
    [InitializeAtRuntime]
    public class ResourceProviderManager : IEngineService
    {
        public event Action<float> OnLoadProgress;
        public event Action<string> OnProviderMessage;
        public event Action<bool> OnLoad;

        public bool IsAnyLoading => providers.Values.Any(p => p.IsLoading);
        public float AverageLoadProgress => providers.Values.Average(p => p.LoadProgress);
        public bool LogResourceLoading => config.LogResourceLoading;
        public ResourcePolicy ResourcePolicy => config.ResourcePolicy;
        public int DynamicPolicySteps => Mathf.Max(1, config.DynamicPolicySteps);

        // Assigned from the editor assembly via reflection when applicaton is executed under Unity editor.
        private static readonly IResourceProvider editorProvider = default;

        private Dictionary<ResourceProviderType, IResourceProvider> providers = new Dictionary<ResourceProviderType, IResourceProvider>();
        private readonly ResourceProviderConfiguration config;

        public ResourceProviderManager (ResourceProviderConfiguration config)
        {
            this.config = config;

            if (ResourcePolicy == ResourcePolicy.Dynamic && config.OptimizeLoadingPriority)
                Application.backgroundLoadingPriority = ThreadPriority.Low;
        }

        public Task InitializeServiceAsync ()
        {
            if (editorProvider != null)
            {
                editorProvider.OnLoadProgress += OnLoadProgress.SafeInvoke;
                editorProvider.OnMessage += (message) => HandleProviderMessage(editorProvider, message);
            }

            OnLoadProgress += HandleOnLoadProgress;
            Application.lowMemory += HandleLowMemory;
            return Task.CompletedTask;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            Application.lowMemory -= HandleLowMemory;
            OnLoadProgress -= HandleOnLoadProgress;
            foreach (var provider in providers.Values)
                provider?.UnloadResources();
            editorProvider?.UnloadResources();
        }

        public bool IsProviderInitialized (ResourceProviderType providerType) => providers.ContainsKey(providerType);

        public IResourceProvider GetProvider (ResourceProviderType providerType)
        {
            if (!providers.ContainsKey(providerType))
                providers[providerType] = InitializeProvider(providerType);
            return providers[providerType];
        }

        public List<IResourceProvider> GetProviderList (params ResourceProviderType[] providerTypes)
        {
            return GetProviderList(providerTypes.ToList());
        }

        public List<IResourceProvider> GetProviderList (List<ResourceProviderType> providerTypes)
        {
            var result = new List<IResourceProvider>();

            // Include editor provider if assigned.
            if (editorProvider != null)
                result.Add(editorProvider);

            // Include requested providers in order.
            foreach (var providerType in providerTypes.Distinct())
            {
                var provider = GetProvider(providerType);
                if (provider != null) result.Add(provider);
            }

            return result;
        }

        private IResourceProvider InitializeProjectProvider ()
        {
            var projectProvider = new ProjectResourceProvider();
            return projectProvider;
        }

        private IResourceProvider InitializeGoogleDriveProvider ()
        {
            #if UNITY_GOOGLE_DRIVE_AVAILABLE
            var gDriveProvider = new GoogleDriveResourceProvider(config.GoogleDriveRootPath, config.GoogleDriveCachingPolicy, config.GoogleDriveRequestLimit);
            gDriveProvider.AddConverter(new JpgOrPngToTextureConverter());
            gDriveProvider.AddConverter(new GDocToScriptAssetConverter());
            gDriveProvider.AddConverter(new Mp3ToAudioClipConverter());
            return gDriveProvider;
            #else
            return null;
            #endif
        }

        private IResourceProvider InitializeLocalProvider ()
        {
            var localProvider = new LocalResourceProvider(config.LocalRootPath);
            localProvider.AddConverter(new JpgOrPngToTextureConverter());
            localProvider.AddConverter(new NaniToScriptAssetConverter());
            localProvider.AddConverter(new WavToAudioClipConverter());
            localProvider.AddConverter(new Mp3ToAudioClipConverter());
            return localProvider;
        }

        private IResourceProvider InitializeProvider (ResourceProviderType providerType)
        {
            IResourceProvider provider;

            switch (providerType)
            {
                case ResourceProviderType.Project:
                    provider = InitializeProjectProvider();
                    break;
                case ResourceProviderType.Local:
                    provider = InitializeLocalProvider();
                    break;
                case ResourceProviderType.GoogleDrive:
                    provider = InitializeGoogleDriveProvider();
                    break;
                default:
                    Debug.LogError($"Failed to initialize provider '{providerType}'.");
                    return null;
            }

            if (provider != null)
            {
                provider.OnLoadProgress += OnLoadProgress.SafeInvoke;
                provider.OnMessage += (message) => HandleProviderMessage(provider, message);
            }

            return provider;
        }

        private void HandleOnLoadProgress (float value)
        {
            OnLoad?.Invoke(IsAnyLoading);
        }

        private void HandleProviderMessage (IResourceProvider provider, string message)
        {
            OnProviderMessage?.Invoke($"[{provider.GetType().Name}] {message}");
        }

        private async void HandleLowMemory ()
        {
            Debug.LogWarning("Forcing resource unloading due to out of memory.");
            await Resources.UnloadUnusedAssets();
        }
    } 
}
