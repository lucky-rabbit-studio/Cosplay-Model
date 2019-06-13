// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="UnityEngine.Animator"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have an <see cref="UnityEngine.Animator"/> component attached to the root object.
    /// All the apperance changes are handled by invoking a <see cref="Animator.SetTrigger(string)"/> with the apperance name as the trigger name.
    /// </remarks>
    public class AnimatorBackground : MonoBehaviourActor, IBackgroundActor
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected Animator Animator { get; private set; }

        private string appearance;
        private bool isVisible;

        public AnimatorBackground (string id, BackgroundMetadata metadata) 
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

        public async Task TransitionAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null)
        {
            await ChangeAppearanceAsync(appearance, duration, easingType);
        }

        public override Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            // TODO: Implement async version.
            SetVisibility(isVisible);
            return Task.CompletedTask;
        }

        public override Task PreloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        public override Task UnloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible)
        {
            this.isVisible = isVisible;

            GameObject?.SetActive(isVisible);
        }

        // TODO: Implement tint color and other props via animator properties.
        protected override Color GetBehaviourTintColor () => Color.white;
        protected override void SetBehaviourTintColor (Color tintColor) { }
    }
}
