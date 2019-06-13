// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages objects spawned with <see cref="Actions.Spawn"/> and <see cref="Actions.SpawnFx"/> actions.
    /// </summary>
    [InitializeAtRuntime]
    public class SpawnManager : IStatefulService<GameStateMap>
    {
        [System.Serializable]
        public class SpawnedObjectState { public string Path; public string[] Params; }

        public class SpawnedObject { public GameObject Object; public SpawnedObjectState State; }

        [System.Serializable]
        private class GameState { public List<SpawnedObjectState> SpawnedObjects; }

        private List<SpawnedObject> spawnedObjects;
        private ResourceProviderManager providersManager;
        private ResourceLoader<GameObject> loader;

        public SpawnManager (ResourceProviderManager providersManager)
        {
            spawnedObjects = new List<SpawnedObject>();
            this.providersManager = providersManager;
        }

        public Task InitializeServiceAsync ()
        {
            loader = new ResourceLoader<GameObject>(new[] { providersManager.GetProvider(ResourceProviderType.Project) });
            return Task.CompletedTask;
        }

        public void ResetService ()
        {
            DestroyAllSpawnedObjects();
        }

        public void DestroyService ()
        {
            DestroyAllSpawnedObjects();
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var state = new GameState() {
                SpawnedObjects = spawnedObjects.Select(o => o.State).ToList()
            };
            stateMap.SerializeObject(state);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            if (state.SpawnedObjects != null)
                foreach (var objState in state.SpawnedObjects)
                    SpawnAsync(objState.Path, objState.Params).WrapAsync();
            return Task.CompletedTask;
        }

        public async Task PreloadObjectAsync (string path)
        {
            await loader.PreloadAsync(path);
        }

        public async Task UnloadObjectAsync (string path)
        {
            if (!loader.IsLoaded(path)) return;

            if (IsObjectSpawned(path))
                DestroySpawnedObject(path);

            await loader.UnloadAsync(path);
        }

        /// <summary>
        /// Attempts to spawn a <see cref="GameObject"/> based on the prefab stored at the provided path.
        /// Used by <see cref="Actions.Spawn"/> action.
        /// </summary>
        /// <returns>Spawned object or null if not spawned.</returns>
        public async Task<SpawnedObject> SpawnAsync (string path, params string[] parameters)
        {
            var prefab = (GameObject)await loader.LoadAsync(path);
            if (!ObjectUtils.IsValid(prefab))
            {
                Debug.LogWarning($"Failed to spawn '{path}': resource not found.");
                return null;
            }

            var obj = Engine.Instantiate(prefab, path);

            var spawnedObj = new SpawnedObject { Object = obj, State = new SpawnedObjectState { Path = path, Params = parameters } };
            spawnedObjects.Add(spawnedObj);

            var parameterized = obj.GetComponent<Actions.Spawn.IParameterized>();
            if (parameterized != null) parameterized.SetSpawnParameters(parameters);

            var awaitable = obj.GetComponent<Actions.Spawn.IAwaitable>();
            if (awaitable != null) await awaitable.AwaitSpawnAsync();

            return spawnedObj;
        }

        /// <summary>
        /// Attempts to destroy a previously spawned <see cref="GameObject"/> with the provided path.
        /// Used by <see cref="Actions.DestroySpawned"/> action.
        /// </summary>
        /// <returns>Whether the object was found and destroyed.</returns>
        public async Task<bool> DestroySpawnedAsync (string path, params string[] parameters)
        {
            var spawnedObj = GetSpawnedObject(path);
            if (spawnedObj is null)
            {
                Debug.LogWarning($"Failed to destroy spawned object '{path}': the object is not found.");
                return false;
            }

            var parameterized = spawnedObj.Object.GetComponent<Actions.DestroySpawned.IParameterized>();
            if (parameterized != null) parameterized.SetDestroyParameters(parameters);

            var awaitable = spawnedObj.Object.GetComponent<Actions.DestroySpawned.IAwaitable>();
            if (awaitable != null) await awaitable.AwaitDestroyAsync();

            return DestroySpawnedObject(path);
        }

        public bool DestroySpawnedObject (string path)
        {
            var spawnedObj = GetSpawnedObject(path);
            if (spawnedObj is null)
            {
                Debug.LogWarning($"Failed to destroy spawned object '{path}': the object is not found.");
                return false;
            }

            var removed = spawnedObjects?.Remove(spawnedObj);
            DestroyObject(spawnedObj.Object);

            return removed ?? false;
        }

        public void DestroyAllSpawnedObjects ()
        {
            foreach (var spawnedObj in spawnedObjects)
                DestroyObject(spawnedObj.Object);
            spawnedObjects.Clear();
        }

        public bool IsObjectSpawned (string path)
        {
            return spawnedObjects?.Exists(o => o.State.Path.EqualsFast(path)) ?? false;
        }

        public SpawnedObject GetSpawnedObject (string path)
        {
            return spawnedObjects?.FirstOrDefault(o => o.State.Path.EqualsFast(path));
        }

        private void DestroyObject (GameObject gameObject)
        {
            if (!ObjectUtils.IsValid(gameObject)) return;

            if (Application.isPlaying) Object.Destroy(gameObject);
            else Object.DestroyImmediate(gameObject);
        }
    }
}
