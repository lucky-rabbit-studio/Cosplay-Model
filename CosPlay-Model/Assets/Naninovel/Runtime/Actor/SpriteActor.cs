// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="MonoBehaviourActor"/> using <see cref="TransitionalSpriteRenderer"/> to represent appearance of the actor.
    /// </summary>
    public abstract class SpriteActor : MonoBehaviourActor
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected LocalizableResourceLoader<Texture2D> AppearanceLoader { get; }
        protected TransitionalSpriteRenderer SpriteRenderer { get; }

        private string appearance;
        private bool isVisible;
        private Resource<Texture2D> defaultAppearance;

        public SpriteActor (string id, OrthoActorMetadata metadata)
            : base(id, metadata)
        {
            AppearanceLoader = ConstructAppearanceLoader(metadata);

            SpriteRenderer = GameObject.AddComponent<TransitionalSpriteRenderer>();
            SpriteRenderer.Pivot = metadata.Pivot;
            SpriteRenderer.PixelsPerUnit = metadata.PixelsPerUnit;

            SetVisibility(false);
        }

        public override async Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            var previousAppearance = this.appearance;
            this.appearance = appearance;

            var textureResource = string.IsNullOrWhiteSpace(appearance) ? await LoadDefaultAppearanceAsync() : await LoadAppearanceAsync(appearance);
            textureResource?.Hold(this);
            await SpriteRenderer.TransitionToAsync(textureResource, duration, easingType);

            // When using `wait:false` this async method won't be waited, which potentially could lead to a situation, where
            // a consequent same method will re-set the currently disposed resource.
            // Here we check that the disposed (previousAppearance) resource is not actually being used right now, before disposing it.
            if (previousAppearance != this.appearance)
                AppearanceLoader?.GetLoadedOrNull(previousAppearance)?.Release(this);
        }

        public override async Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            // When appearance is not set (and default one is not preloaded for some reason, eg when using dynamic parameters) 
            // and revealing the actor -- attempt to load default appearance texture.
            if (!IsVisible && isVisible && string.IsNullOrWhiteSpace(Appearance) && !defaultAppearance.IsValid)
                await ChangeAppearanceAsync(null, 0);

            this.isVisible = isVisible;

            await SpriteRenderer.FadeToAsync(isVisible ? TintColor.a : 0, duration, easingType);
        }

        public override async Task HoldResourcesAsync (object holder, string appearance)
        {
            if (string.IsNullOrEmpty(appearance))
            {
                await LoadDefaultAppearanceAsync();
                defaultAppearance?.Hold(holder);
                return;
            }

            var resource = await AppearanceLoader.LoadAsync(appearance);
            if (resource.IsValid)
                resource.Hold(holder);
        }

        public override void ReleaseResources (object holder, string appearance)
        {
            if (string.IsNullOrEmpty(appearance)) return;

            AppearanceLoader.GetLoadedOrNull(appearance)?.Release(holder);
        }

        public override void Dispose ()
        {
            base.Dispose();

            AppearanceLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
        }

        protected virtual LocalizableResourceLoader<Texture2D> ConstructAppearanceLoader (OrthoActorMetadata metadata)
        {
            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var localeMngr = Engine.GetService<LocalizationManager>();
            var appearanceLoader = new LocalizableResourceLoader<Texture2D>(
                providerMngr.GetProviderList(metadata.LoaderConfiguration.ProviderTypes),
                localeMngr, $"{metadata.LoaderConfiguration.PathPrefix}/{Id}");

            return appearanceLoader;
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible) => ChangeVisibilityAsync(isVisible, 0).WrapAsync();

        protected override Color GetBehaviourTintColor () => SpriteRenderer.TintColor;

        protected override void SetBehaviourTintColor (Color tintColor)
        {
            if (!IsVisible) // Handle visibility-controlled alpha of the tint color.
                tintColor.a = SpriteRenderer.TintColor.a;
            SpriteRenderer.TintColor = tintColor;
        }

        protected virtual async Task<Resource<Texture2D>> LoadAppearanceAsync (string appearance)
        {
            var texture = await AppearanceLoader.LoadAsync(appearance);

            if (!texture.IsValid)
            {
                Debug.LogWarning($"Failed to load '{appearance}' appearance texture for `{Id}` sprite actor: the resource is not found.");
                return null;
            }

            ApplyTextureSettings(texture);
            return texture;
        }

        protected virtual async Task<Resource<Texture2D>> LoadDefaultAppearanceAsync ()
        {
            if (defaultAppearance != null && defaultAppearance.IsValid) return defaultAppearance;

            var defaultTexturePath = await LocateDefaultAppearanceAsync();
            defaultAppearance = defaultTexturePath is null ? new Resource<Texture2D>(null, Texture2D.whiteTexture, null) : await AppearanceLoader.LoadAsync(defaultTexturePath);

            ApplyTextureSettings(defaultAppearance);

            if (!SpriteRenderer.MainTexture)
                SpriteRenderer.MainTexture = defaultAppearance;

            return defaultAppearance;
        }

        protected virtual async Task<string> LocateDefaultAppearanceAsync ()
        {
            var texturePaths = (await AppearanceLoader.LocateAsync(string.Empty))?.ToList();
            if (texturePaths != null && texturePaths.Count > 0)
            {
                // Remove path prefix (caller is expecting a local path).
                for (int i = 0; i < texturePaths.Count; i++)
                    if (texturePaths[i].Contains($"{AppearanceLoader.PathPrefix}/"))
                        texturePaths[i] = texturePaths[i].Replace($"{AppearanceLoader.PathPrefix}/", string.Empty);

                if (texturePaths.Any(t => t.EndsWithFast(Id)))
                    return texturePaths.First(t => t.EndsWithFast(Id));

                if (texturePaths.Any(t => t.EndsWithFast("Default")))
                    return texturePaths.First(t => t.EndsWithFast("Default"));
            }

            Debug.LogWarning($"Failed to locate a default appearance texture for `{Id}` sprite actor.");
            return null;
        }

        protected virtual void ApplyTextureSettings (Texture2D texture)
        {
            if (texture && texture.wrapMode != TextureWrapMode.Clamp)
                texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
