// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IActor"/> implementation using <typeparamref name="TBehaviour"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have a <typeparamref name="TBehaviour"/> component attached to the root object.
    /// Apperance and other property changes changes are routed to the events of the <typeparamref name="TBehaviour"/> component.
    /// </remarks>
    public abstract class GenericActor<TBehaviour> : MonoBehaviourActor
        where TBehaviour : GenericActorBehaviour
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected TBehaviour Behaviour { get; private set; }

        private ActorMetadata metadata;
        private string appearance;
        private bool isVisible;
        private Color tintColor = Color.white;

        public GenericActor (string id, ActorMetadata metadata)
            : base(id, metadata)
        {
            this.metadata = metadata;
        }

        public override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var localeMngr = Engine.GetService<LocalizationManager>();
            var prefabResource = await new LocalizableResourceLoader<TBehaviour>(
                providerMngr.GetProviderList(ResourceProviderType.Project),
                localeMngr, metadata.LoaderConfiguration.PathPrefix).LoadAsync(Id);

            Behaviour = Engine.Instantiate(prefabResource.Object);
            Behaviour.transform.SetParent(Transform);

            SetVisibility(false);
        }

        public override Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            this.appearance = appearance;

            if (string.IsNullOrEmpty(appearance))
                return Task.CompletedTask;

            Behaviour.InvokeAppearanceChangedEvent(appearance);

            return Task.CompletedTask;
        }

        public override Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            SetVisibility(isVisible);
            return Task.CompletedTask;
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible)
        {
            this.isVisible = isVisible;

            Behaviour.InvokeVisibilityChangedEvent(isVisible);
        }

        protected override Color GetBehaviourTintColor () => tintColor;

        protected override void SetBehaviourTintColor (Color tintColor)
        {
            this.tintColor = tintColor;

            Behaviour.InvokeTintColorChangedEvent(tintColor);
        }
    }
}
