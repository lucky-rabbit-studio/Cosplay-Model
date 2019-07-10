// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages <see cref="IManagedUI"/> objects.
    /// </summary>
    [InitializeAtRuntime(2)] // Managed UIs could potentially use any other services, so initialize at the very end.
    public class UIManager : IEngineService
    {
        private class ManagedUI
        {
            public readonly string Id;
            public readonly string PrefabName;
            public readonly GameObject GameObject;
            public readonly IManagedUI UIComponent;
            public readonly Type ComponentType;

            public ManagedUI (string prefabName, GameObject gameObject, IManagedUI uiComponent)
            {
                PrefabName = prefabName;
                GameObject = gameObject;
                UIComponent = uiComponent;
                ComponentType = UIComponent?.GetType();
                Id = $"{PrefabName}<{ComponentType.FullName}>";
            }
        }

        public int ObjectLayer => config.ObjectsLayer;
        /// <summary>
        /// Whether to show the UI. When hidden, a <see cref="ClickThroughPanel"/> will be activated
        /// allowing the user to show the UI back by clicking anywhere on the screen or by activating the `ToggleUI` and `Submit` inputs.
        /// </summary>
        public bool UIVisible { get => cameraManager.RenderUI; set => SetUIVisible(value); }

        private const string defaultUIResourcesPath = "Naninovel/DefaultUI";

        private readonly UIConfiguration config;
        private CameraManager cameraManager;
        private InputManager inputManager;
        private Camera customCamera;
        private List<ManagedUI> managedUIs;

        public UIManager (UIConfiguration config, CameraManager cameraManager, InputManager inputManager)
        {
            this.config = config;
            this.cameraManager = cameraManager;
            this.inputManager = inputManager;
            managedUIs = new List<ManagedUI>();
        }

        public async Task InitializeServiceAsync ()
        {
            foreach (var prefab in config.CustomUI)
                InstantiateUIPrefab(prefab);

            var existingUIs = managedUIs.Select(ui => ui.UIComponent);
            var defaultUIs = LoadUniqueDefaultUIs(existingUIs);
            foreach (var uiPrefab in defaultUIs)
                InstantiateUIPrefab(uiPrefab);

            existingUIs = managedUIs.Select(ui => ui.UIComponent);
            await Task.WhenAll(existingUIs.Select(ui => ui.InitializeAsync()));

            inputManager.ToggleUI.OnStart += ToggleUI;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            inputManager.ToggleUI.OnStart -= ToggleUI;

            foreach (var managedUI in managedUIs)
            {
                if (!ObjectUtils.IsValid(managedUI.GameObject)) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(managedUI.GameObject);
                else UnityEngine.Object.DestroyImmediate(managedUI.GameObject);
            }
            managedUIs.Clear();
        }

        /// <summary>
        /// Instatiates the provided prefab, adds a <see cref="IManagedUI"/> component
        /// attached to the root object to the managed objects and applies configuration properties.
        /// Don't forget to manually invoke <see cref="IManagedUI.InitializeAsync"/> on the returned object if required.
        /// </summary>
        /// <param name="prefab">The prefab to spawn. Should have a <see cref="IManagedUI"/> component attached to the root object.</param>
        public IManagedUI InstantiateUIPrefab (GameObject prefab)
        {
            var gameObject = Engine.Instantiate(prefab, prefab.name, ObjectLayer);
            var uiComponent = gameObject.GetComponent<IManagedUI>();
            if (uiComponent is null) return null;

            uiComponent.SortingOrder += config.SortingOffset;
            uiComponent.RenderMode = config.RenderMode;
            uiComponent.RenderCamera = ObjectUtils.IsValid(customCamera) ? customCamera : ObjectUtils.IsValid(cameraManager.UICamera) ? cameraManager.UICamera : cameraManager.Camera;

            var managedUI = new ManagedUI(prefab.name, gameObject, uiComponent);
            managedUIs.Add(managedUI);

            return uiComponent;
        }

        /// <summary>
        /// Returns a managed UI of the provided type <typeparamref name="T"/>.
        /// </summary>
        public T GetUI<T> () where T : class, IManagedUI => GetUI(typeof(T)) as T;

        /// <summary>
        /// Returns a managed UI of the provided type.
        /// </summary>
        public IManagedUI GetUI (Type type)
        {
            foreach (var managedUI in managedUIs)
                if (type.IsAssignableFrom(managedUI.ComponentType))
                    return managedUI.UIComponent;
            return null;
        }

        /// <summary>
        /// Returns a managed UI based on the provided prefab name;
        /// </summary>
        public IManagedUI GetUI (string prefabName)
        {
            foreach (var managedUI in managedUIs)
                if (managedUI.PrefabName == prefabName)
                    return managedUI.UIComponent;
            return null;
        }

        /// <summary>
        /// Applies provided render mode and camera for all the managed UI objects.
        /// </summary>
        public void SetRenderMode (RenderMode renderMode, Camera renderCamera)
        {
            customCamera = renderCamera;
            foreach (var managedUI in managedUIs)
            {
                managedUI.UIComponent.RenderMode = renderMode;
                managedUI.UIComponent.RenderCamera = renderCamera;
            }
        }

        private void ToggleUI () => SetUIVisible(!UIVisible);

        private void SetUIVisible (bool visible)
        {
            cameraManager.RenderUI = visible;
            if (visible) GetUI<ClickThroughPanel>()?.Hide();
            else GetUI<ClickThroughPanel>()?.Show(true, ToggleUI, InputManager.SubmitName, InputManager.ToggleUIName);
        }

        /// <summary>
        /// Loads default UI prefabs with <see cref="IManagedUI"/> implementations that doesn't exist in the provided <paramref name="existingUIs"/>.
        /// </summary>
        private static IEnumerable<GameObject> LoadUniqueDefaultUIs (IEnumerable<IManagedUI> existingUIs)
        {
            IEnumerable<Type> GetImplementation (Type type) => type?.GetInterfaces()?.Where(i => typeof(IManagedUI).IsAssignableFrom(i));

            bool FilterByImplementation (GameObject obj)
            {
                var objImpl = GetImplementation(obj?.GetComponent<IManagedUI>()?.GetType());
                if (objImpl is null) return true;
                return !existingUIs.Any(e => GetImplementation(e.GetType()).SequenceEqual(objImpl));
            }

            var defaultPrefabs = Resources.LoadAll<GameObject>(defaultUIResourcesPath);
            return defaultPrefabs.Where(FilterByImplementation);
        }
    }
}
