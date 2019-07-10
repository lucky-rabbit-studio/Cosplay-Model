// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ICharacterActor"/> implementation using <see cref="CharacterActorBehaviour"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have a <see cref="CharacterActorBehaviour"/> component attached to the root object.
    /// Apperance and other property changes changes are routed to the events of the <see cref="CharacterActorBehaviour"/> component.
    /// </remarks>
    public class GenericCharacter : GenericActor<CharacterActorBehaviour>, ICharacterActor
    {
        public CharacterLookDirection LookDirection { get => lookDirection; set => SetLookDirection(value); }

        private CharacterLookDirection lookDirection;

        public GenericCharacter (string id, CharacterMetadata metadata)
            : base(id, metadata) { }

        public async Task ChangeLookDirectionAsync (CharacterLookDirection lookDirection, float duration, EasingType easingType = default)
        {
            this.lookDirection = lookDirection;

            Behaviour.InvokeLookDirectionChangedEvent(lookDirection);

            if (Behaviour.TransformByLookDirection)
            {
                var rotation = LookDirectionToRotation(lookDirection);
                await ChangeRotationAsync(rotation, duration, easingType);
            }
        }

        protected virtual void SetLookDirection (CharacterLookDirection lookDirection)
        {
            this.lookDirection = lookDirection;

            Behaviour.InvokeLookDirectionChangedEvent(lookDirection);

            if (Behaviour.TransformByLookDirection)
            {
                var rotation = LookDirectionToRotation(lookDirection);
                SetBehaviourRotation(rotation);
            }
        }

        protected virtual Quaternion LookDirectionToRotation (CharacterLookDirection lookDirection)
        {
            var yAngle = 0f;
            switch (lookDirection)
            {
                case CharacterLookDirection.Center:
                    yAngle = 0;
                    break;
                case CharacterLookDirection.Left:
                    yAngle = Behaviour.LookDeltaAngle;
                    break;
                case CharacterLookDirection.Right:
                    yAngle = -Behaviour.LookDeltaAngle;
                    break;
            }

            var currentRotation = Rotation.eulerAngles;
            return Quaternion.Euler(currentRotation.x, yAngle, currentRotation.z);
        }
    }
}
