// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ICharacterActor"/> implementation using <see cref="UnityEngine.Animator"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// Prefab with the actor name should have an <see cref="UnityEngine.Animator"/> component attached to the root object.
    /// All the apperance changes are handled by invoking a <see cref="Animator.SetTrigger(string)"/> with the apperance name as the trigger name.
    /// </remarks>
    public class AnimatorCharacter : MonoBehaviourActor, ICharacterActor
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }
        public CharacterLookDirection LookDirection { get => lookDirection; set => SetLookDirection(value); }

        protected Animator Animator { get; private set; }

        private const float lookDeltaAngle = 30;
        private string appearance;
        private bool isVisible;
        private CharacterLookDirection lookDirection;

        public AnimatorCharacter (string id, CharacterMetadata metadata) 
            : base(id, metadata)
        {
            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var localeMngr = Engine.GetService<LocalizationManager>();
            var prefabResource = new LocalizableResourceLoader<Animator>(
                providerMngr.GetProviderList(ResourceProviderType.Project),
                localeMngr, metadata.LoaderConfiguration.PathPrefix).Load(id);

            Animator = Engine.Instantiate(prefabResource.Object);
            Animator.transform.SetParent(Transform);

            SetVisibility(false);
        }

        public override Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            this.appearance = appearance;

            if (string.IsNullOrEmpty(appearance)) return Task.CompletedTask;

            Animator.SetTrigger(appearance);

            return Task.CompletedTask;
        }

        public override Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            // TODO: Implement async version.
            SetVisibility(isVisible);
            return Task.CompletedTask;
        }

        public async Task ChangeLookDirectionAsync (CharacterLookDirection lookDirection, float duration, EasingType easingType = default)
        {
            this.lookDirection = lookDirection;

            var rotation = LookDirectionToRotation(lookDirection);
            await ChangeRotationAsync(rotation, duration, easingType);
        }

        public override Task PreloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        public override Task UnloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible)
        {
            this.isVisible = isVisible;

            GameObject?.SetActive(isVisible);
        }

        protected virtual void SetLookDirection (CharacterLookDirection lookDirection)
        {
            this.lookDirection = lookDirection;

            var rotation = LookDirectionToRotation(lookDirection);
            SetBehaviourRotation(rotation);
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
                    yAngle = lookDeltaAngle;
                    break;
                case CharacterLookDirection.Right:
                    yAngle = -lookDeltaAngle;
                    break;
            }

            var currentRotation = Rotation.eulerAngles;
            return Quaternion.Euler(currentRotation.x, yAngle, currentRotation.z);
        }

        // TODO: Implement tint color and other props via animator properties.
        protected override Color GetBehaviourTintColor () => Color.white;
        protected override void SetBehaviourTintColor (Color tintColor) { }
    }
}
