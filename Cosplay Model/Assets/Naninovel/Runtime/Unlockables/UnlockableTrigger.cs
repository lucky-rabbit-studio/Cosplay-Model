// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityCommon;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Allows to listen for events when an unlockable item managed by <see cref="UnlockableManager"/> is updated.
    /// </summary>
    public class UnlockableTrigger : MonoBehaviour
    {
        [Serializable]
        private class UnlockedStateChangedEvent : UnityEvent<bool> { }

        /// <summary>
        /// Invoked when unlocked state of the listened unlockable item is changed.
        /// </summary>
        public event Action<bool> OnUnlockedStateChanged;

        /// <summary>
        /// ID of the unlockable item to listen for.
        /// </summary>
        public string UnlockableItemId { get => unlockableItemId; set => unlockableItemId = value; }
        /// <summary>
        /// Attempts to retrieve current unlockable state of the listened item.
        /// </summary>
        public bool UnlockedState => unlockableManager.ItemUnlocked(UnlockableItemId);

        [Tooltip("ID of the unlockable item to listen for.")]
        [SerializeField] private string unlockableItemId = default;
        [Tooltip("Invoked when unlocked state of the listened unlockable item is changed; also invoked when the component is started.")]
        [SerializeField] private UnlockedStateChangedEvent onUnlockedStateChanged = default;

        private UnlockableManager unlockableManager;

        private void Awake ()
        {
            unlockableManager = Engine.GetService<UnlockableManager>();
        }

        private void OnEnable ()
        {
            unlockableManager.OnItemUpdated += HandleItemUpdated;
        }

        private void OnDisable ()
        {
            if (unlockableManager != null)
                unlockableManager.OnItemUpdated -= HandleItemUpdated;
        }

        private void Start ()
        {
            OnUnlockedStateChanged?.Invoke(UnlockedState);
            onUnlockedStateChanged?.Invoke(UnlockedState);
        }

        private void HandleItemUpdated (UnlockableItemUpdatedArgs args)
        {
            if (!args.Id.EqualsFastIgnoreCase(UnlockableItemId)) return;

            OnUnlockedStateChanged?.Invoke(args.Unlocked);
            onUnlockedStateChanged?.Invoke(args.Unlocked);
        }
    }
}
