// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Destroys object spawned with [`@spawn`](/api/#spawn) action.
    /// </summary>
    /// <remarks>
    /// If prefab has a <see cref="MonoBehaviour"/> component attached the root object, and the component implements
    /// a <see cref="IParameterized"/> interface, will pass the specified `params` values before destroying the object;
    /// if the component implements <see cref="IAwaitable"/> interface, action execution will wait for
    /// the async completion task returned by the implementation before destroying the object.
    /// </remarks>
    /// <example>
    /// ; Given a "@spawn Rainbow" action was executed before
    /// @despawn Rainbow
    /// </example>
    [ActionAlias("despawn")]
    public class DestroySpawned : NovelAction
    {
        private struct UndoData { public bool Destroyed; public string[] SpawnParams; }

        public interface IParameterized { void SetDestroyParameters (string[] parameters); }
        public interface IAwaitable { Task AwaitDestroyAsync (); }

        /// <summary>
        /// Path to the prefab resource to destroy. Path is relative to a `./Resources` folder, eg 
        /// given a `Assets/Resources/FX/Explosion.prefab` asset, use the following path to spawn it: `FX/Explosion`.
        /// A [`@spawn`](/api/#spawn) action with the same path is expected to be executed before.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string Path { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Parameters to set before destoying the prefab.
        /// Requires the prefab to have a <see cref="IParameterized"/> component attached the root object.
        /// </summary>
        [ActionParameter(optional: true)]
        public string[] Params { get => GetDynamicParameter<string[]>(null); set => SetDynamicParameter(value); }

        protected virtual string FullPath => Path;
        protected virtual SpawnManager SpawnManager => Engine.GetService<SpawnManager>();

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var spawnedObj = SpawnManager.GetSpawnedObject(FullPath);
            if (spawnedObj is null)
            {
                Debug.LogWarning($"Failed to destroy spawned object '{FullPath}': the object is not found.");
                return;
            }

            var destroyed = await SpawnManager.DestroySpawnedAsync(FullPath, Params);
            if (destroyed)
            {
                undoData.Destroyed = true;
                undoData.SpawnParams = spawnedObj.State.Params;
            }
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Destroyed) return;

            await SpawnManager.SpawnAsync(FullPath, undoData.SpawnParams);

            undoData = default;
        }
    }
}
