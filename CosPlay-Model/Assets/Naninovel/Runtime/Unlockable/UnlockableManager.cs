// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Manages unlockable items (CG and movie gallery items, tips, etc).
    /// </summary>
    [InitializeAtRuntime]
    public class UnlockableManager : IEngineService, IStatefulService<GlobalStateMap>
    {
        /// <summary>
        /// Serializable dirctionary, representing unlockable item ID to its unlocked state map.
        /// </summary>
        [Serializable]
        public class UnlockablesMap : SerializableMap<string, bool>
        {
            public UnlockablesMap () : base(StringComparer.OrdinalIgnoreCase) { }
        }

        [Serializable]
        private class GlobalState
        {
            public UnlockablesMap UnlockablesMap = new UnlockablesMap();
        }

        /// <summary>
        /// Invoked when unlocked state of an unlockable item is changed (or when it's added to the map for the first time).
        /// </summary>
        public event Action<UnlockableItemUpdatedArgs> OnItemUpdated;

        private readonly UnlockablesConfiguration config;
        private UnlockablesMap unlockablesMap;

        public UnlockableManager (UnlockablesConfiguration config)
        {
            this.config = config;
            unlockablesMap = new UnlockablesMap();
        }

        public Task InitializeServiceAsync () => Task.CompletedTask;

        public void ResetService () { }

        public void DestroyService () { }

        public Task SaveServiceStateAsync (GlobalStateMap stateMap)
        {
            var globalState = new GlobalState {
                UnlockablesMap = unlockablesMap
            };
            stateMap.SerializeObject(globalState);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GlobalStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GlobalState>() ?? new GlobalState();
            unlockablesMap = state.UnlockablesMap;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks whether unlockable item with the provided ID exists and is unlocked.
        /// </summary>
        public bool ItemUnlocked (string itemId) => unlockablesMap.TryGetValue(itemId, out var item) && item;

        /// <summary>
        /// Modifies unlockable state for an unlockable item with the provided ID.
        /// In case item with the provided ID doesn't exist, will add it to the map.
        /// </summary>
        public void SetItemUnlocked (string itemId, bool unlocked)
        {
            if (unlocked && ItemUnlocked(itemId)) return;
            if (!unlocked && unlockablesMap.ContainsKey(itemId) && !ItemUnlocked(itemId)) return;

            var added = unlockablesMap.ContainsKey(itemId);
            unlockablesMap[itemId] = unlocked;
            OnItemUpdated?.Invoke(new UnlockableItemUpdatedArgs(itemId, unlocked, added));
        }

        /// <summary>
        /// Makes unlockable item with the provided ID unlocked.
        /// In case item with the provided ID doesn't exist, will add it to the map.
        /// </summary>
        public void UnlockItem (string itemId) => SetItemUnlocked(itemId, true);

        /// <summary>
        /// Makes unlockable item with the provided ID locked.
        /// In case item with the provided ID doesn't exist, will add it to the map.
        /// </summary>
        public void LockItem (string itemId) => SetItemUnlocked(itemId, false);

        /// <summary>
        /// Returns all the stored unlockable item records as item ID to unlocked state map.
        /// </summary>
        public Dictionary<string, bool> GetAllItems () => unlockablesMap.ToDictionary(kv => kv.Key, kv => kv.Value);

        /// <summary>
        /// Makes all the stored unlockable items unlocked.
        /// </summary>
        public void UnlockAllItems ()
        {
            foreach (var itemId in unlockablesMap.Keys)
                UnlockItem(itemId);
        }

        /// <summary>
        /// Makes all the stored unlockable items locked.
        /// </summary>
        public void LockAllItems ()
        {
            foreach (var itemId in unlockablesMap.Keys)
                LockItem(itemId);
        }
    }
}
