// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Hosts events routed by <see cref="GenericActor{TBehaviour}"/>.
    /// </summary>
    public class CharacterActorBehaviour : GenericActorBehaviour
    {
        [System.Serializable]
        private class LookDirectionChangedEvent : UnityEvent<CharacterLookDirection> { }

        /// <summary>
        /// Invoked when look direction of the character is changed.
        /// </summary>
        public event Action<CharacterLookDirection> OnLookDirectionChanged;

        public bool TransformByLookDirection => transformByLookDirection;
        public float LookDeltaAngle => lookDeltaAngle;

        [Tooltip("Invoked when look direction of the character is changed.")]
        [SerializeField] private LookDirectionChangedEvent onLookDirectionChanged = default;
        [Tooltip("Whether to react to look direction changes by rotating the object's transform.")]
        [SerializeField] private bool transformByLookDirection = true;
        [Tooltip("When `" + nameof(transformByLookDirection) + "` is enabled, controls the rotation angle.")]
        [SerializeField] private float lookDeltaAngle = 30;

        public void InvokeLookDirectionChangedEvent (CharacterLookDirection value)
        {
            OnLookDirectionChanged?.Invoke(value);
            onLookDirectionChanged?.Invoke(value);
        }
    }
}
