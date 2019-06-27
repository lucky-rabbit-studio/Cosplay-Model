// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Class responsible for management of systems critical to the engine.
    /// </summary>
    public static class Engine
    {
        public static event Action OnInitialized;

        public static GameObject RootObject => Behaviour.GetRootObject();
        public static IEngineBehaviour Behaviour { get; private set; }
        public static bool IsInitialized => initializeTCS != null && initializeTCS.Task.IsCompleted;
        public static bool IsInitializing => initializeTCS != null && !initializeTCS.Task.IsCompleted;
        public static bool OverrideObjectsLayer => config.OverrideObjectsLayer;
        public static int ObjectsLayer => config.ObjectsLayer;

        private static EngineConfiguration config;
        private static List<IEngineService> services;
        private static TaskCompletionSource<object> initializeTCS;

        /// <summary>
        /// Initializes engine behaviour and services.
        /// Services will be initialized in the order in which they were added to the list.
        /// </summary>
        public static async Task InitializeAsync (EngineConfiguration config, IEngineBehaviour behaviour, List<IEngineService> services)
        {
            if (IsInitialized) return;
            if (IsInitializing) { await initializeTCS.Task; return; }

            Engine.config = config;
            initializeTCS = new TaskCompletionSource<object>();

            Behaviour = behaviour;
            Behaviour.OnBehaviourDestroy += Destroy;

            Engine.services = services;
            foreach (var service in services)
                await service.InitializeServiceAsync();

            initializeTCS.TrySetResult(null);
            OnInitialized?.Invoke();
        }

        /// <summary>
        /// Resets state of all the engine services.
        /// </summary>
        public static void Reset () => services?.ForEach(s => s.ResetService());

        /// <summary>
        /// Deconstructs all the engine services and stops the behaviour.
        /// </summary>
        public static void Destroy ()
        {
            initializeTCS = null;

            services?.ForEach(s => s.DestroyService());
            services = null;

            if (Behaviour != null)
            {
                Behaviour.OnBehaviourDestroy -= Destroy;
                Behaviour.Destroy();
                Behaviour = null;
            }
        }

        /// <summary>
        /// Resolves a <see cref="IEngineService"/> object from the services list.
        /// </summary>
        public static TService GetService<TService> (Predicate<TService> predicate = null, bool assertResult = true)
            where TService : class, IEngineService
        {
            if (services is null) return null;

            var resolvingType = typeof(TService);
            foreach (var service in services)
            {
                if (!resolvingType.IsAssignableFrom(service.GetType())) continue;
                if (predicate != null && !predicate(service as TService)) continue;
                return service as TService;
            }

            Debug.LogError($"Failed to resolve service of type '{resolvingType}': service not found.");
            return null;
        }

        /// <summary>
        /// Resolves all the matching <see cref="IEngineService"/> objects from the services list.
        /// </summary>
        public static List<TService> GetAllServices<TService> (Predicate<TService> predicate = null, bool assertResult = true) 
            where TService : class, IEngineService
        {
            var result = new List<TService>();
            var resolvingType = typeof(TService);

            var servicesOfType = services?.FindAll(s => resolvingType.IsAssignableFrom(s.GetType()));
            if (servicesOfType != null && servicesOfType.Count > 0)
                result = servicesOfType.FindAll(s => predicate is null || predicate(s as TService)).Cast<TService>().ToList();

            if (result is null && assertResult)
                Debug.LogError($"Failed to resolve service of type '{resolvingType}': service not found.");

            return result;
        }

        /// <summary>
        /// Invokes <see cref="UnityEngine.Object.Instantiate(UnityEngine.Object)"/> and adds the object as child of the engine object.
        /// </summary>
        /// <param name="prototype">Prototype of the object to instantiate.</param>
        /// <param name="name">Name to assign for the instantiated object. Will use name of the prototype when not provided.</param>
        /// <param name="layer">Layer to assign for the instantiated object. Will assign <see cref="ObjectsLayer"/> (when <see cref="OverrideObjectsLayer"/>, otherwise will preserve prototype's layer) when not provided or less than zero.</param>
        public static T Instantiate<T> (T prototype, string name = null, int layer = -1) where T : UnityEngine.Object
        {
            if (Behaviour is null)
            {
                Debug.LogError($"Failed to instatiate `{name ?? prototype.name}`: engine is not ready. Make sure you're not using this inside an engine service constructor (use InitializeServiceAsync() instead).");
                return null;
            }

            var newObj = UnityEngine.Object.Instantiate(prototype);
            var gameObj = newObj as GameObject ?? (newObj as Component).gameObject;
            Behaviour.AddChildObject(gameObj);

            if (!string.IsNullOrEmpty(name)) newObj.name = name;

            if (layer > -1) gameObj.ForEachDescendant(obj => obj.layer = layer);
            else if (OverrideObjectsLayer) gameObj.ForEachDescendant(obj => obj.layer = ObjectsLayer);

            return newObj;
        }

        /// <summary>
        /// Creates a new <see cref="GameObject"/>, making it a child of the engine object and (optionally) adding provided components.
        /// </summary>
        /// <param name="name">Name to assign for the instantiated object. Will use a default name when not provided.</param>
        /// <param name="layer">Layer to assign for the instantiated object. Will assign <see cref="ObjectsLayer"/> (when <see cref="OverrideObjectsLayer"/>, otherwise will preserve prototype's layer) when not provided or less than zero.</param>
        /// <param name="components">Components to add on the created object.</param>
        public static GameObject CreateObject (string name = null, int layer = -1, params Type[] components)
        {
            if (Behaviour is null)
            {
                Debug.LogError($"Failed to create `{name ?? string.Empty}` object: engine is not ready. Make sure you're not using this inside an engine service constructor (use InitializeServiceAsync() instead).");
                return null;
            }

            var objName = name ?? "NaninovelObject";
            GameObject newObj;
            if (components != null) newObj = new GameObject(objName, components);
            else newObj = new GameObject(objName);
            Behaviour.AddChildObject(newObj);

            if (layer > -1) newObj.ForEachDescendant(obj => obj.layer = layer);
            else if (OverrideObjectsLayer) newObj.ForEachDescendant(obj => obj.layer = ObjectsLayer);

            return newObj;
        }

        /// <summary>
        /// Creates a new <see cref="GameObject"/>, making it a child of the engine object and adding specified component type.
        /// </summary>
        /// <param name="name">Name to assign for the instantiated object. Will use a default name when not provided.</param>
        /// <param name="layer">Layer to assign for the instantiated object. Will assign <see cref="ObjectsLayer"/> (when <see cref="OverrideObjectsLayer"/>, otherwise will preserve prototype's layer) when not provided or less than zero.</param>
        public static T CreateObject<T> (string name = null, int layer = -1) where T : Component
        {
            if (Behaviour is null)
            {
                Debug.LogError($"Failed to create `{name ?? string.Empty}` object of type `{typeof(T).Name}`: engine is not ready. Make sure you're not using this inside an engine service constructor (use InitializeServiceAsync() instead).");
                return null;
            }

            var newObj = new GameObject(name ?? typeof(T).Name);
            Behaviour.AddChildObject(newObj);

            if (layer > -1) newObj.ForEachDescendant(obj => obj.layer = layer);
            else if (OverrideObjectsLayer) newObj.ForEachDescendant(obj => obj.layer = ObjectsLayer);

            return newObj.AddComponent<T>();
        }
    }
}
