// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="MonoBehaviourActor"/> using <see cref="NovelSpriteRenderer"/> to represent appearance of the actor.
    /// </summary>
    public abstract class SpriteActor : MonoBehaviourActor
    {
        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected LocalizableResourceLoader<Texture2D> AppearanceLoader { get; }
        protected NovelSpriteRenderer SpriteRenderer { get; }

        private string appearance;
        private bool isVisible;
        private Texture2D defaultTexture;

        public SpriteActor (string id, OrthoActorMetadata metadata)
            : base(id, metadata)
        {
            AppearanceLoader = ConstructAppearanceLoader(metadata);

            SpriteRenderer = GameObject.AddComponent<NovelSpriteRenderer>();
            SpriteRenderer.Pivot = metadata.Pivot;
            SpriteRenderer.PixelsPerUnit = metadata.PixelsPerUnit;

            SetVisibility(false);
        }

        public override async Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            this.appearance = appearance;

            var texture = string.IsNullOrWhiteSpace(appearance) ? await LoadDefaultTextureAsync() : await LoadAppearanceTextureAsync(appearance);
            await SpriteRenderer.TransitionToAsync(texture, duration, easingType);
        }

        public override async Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            // When appearance is not set (and default one is not preloaded for some reason, eg when using dynamic parameters) 
            // and revealing the actor -- attempt to load default appearance texture.
            if (!IsVisible && isVisible && string.IsNullOrWhiteSpace(Appearance) && !ObjectUtils.IsValid(defaultTexture))
                await ChangeAppearanceAsync(null, 0);

            this.isVisible = isVisible;

            await SpriteRenderer.FadeToAsync(isVisible ? TintColor.a : 0, duration, easingType);
        }

        public override async Task PreloadResourcesAsync (string appearance = null)
        {
            if (string.IsNullOrEmpty(appearance)) { await LoadDefaultTextureAsync(); return; }

            await AppearanceLoader.PreloadAsync(appearance);
        }

        public override async Task UnloadResourcesAsync (string appearance = null)
        {
            if (string.IsNullOrEmpty(appearance))
            {
                await AppearanceLoader.UnloadAllAsync();
                return;
            }

            await AppearanceLoader.UnloadAsync(appearance);
            return;
        }

        public override void Dispose ()
        {
            base.Dispose();

            AppearanceLoader?.UnloadAllAsync();
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

        protected virtual async Task<Texture2D> LoadAppearanceTextureAsync (string appearance)
        {
            var texture = await AppearanceLoader.LoadAsync(appearance);
            ApplyTextureSettings(texture);
            return texture;
        }

        protected virtual async Task<Texture2D> LoadDefaultTextureAsync ()
        {
            if (defaultTexture) return defaultTexture;

            var defaultTexturePath = await LocateDefaultTextureAsync();
            defaultTexture = defaultTexturePath is null ? Texture2D.whiteTexture : await AppearanceLoader.LoadAsync(defaultTexturePath);

            ApplyTextureSettings(defaultTexture);

            if (!SpriteRenderer.MainTexture)
                SpriteRenderer.MainTexture = defaultTexture;

            return defaultTexture;
        }

        protected virtual async Task<string> LocateDefaultTextureAsync ()
        {
            var texturePaths = (await AppearanceLoader.LocateResourcesAsync(string.Empty))?.Select(r => r.Path).ToList();
            if (texturePaths is null || texturePaths.Count == 0) return null;

            // Remove path prefix (caller is expecting a local path).
            for (int i = 0; i < texturePaths.Count; i++)
                if (texturePaths[i].Contains($"{AppearanceLoader.PathPrefix}/"))
                    texturePaths[i] = texturePaths[i].Replace($"{AppearanceLoader.PathPrefix}/", string.Empty);

            if (texturePaths.Any(t => t.EndsWithFast(Id)))
                return texturePaths.First(t => t.EndsWithFast(Id));

            if (texturePaths.Any(t => t.EndsWithFast("Default")))
                return texturePaths.First(t => t.EndsWithFast("Default"));

            return null;
        }

        protected virtual void ApplyTextureSettings (Texture2D texture)
        {
            if (texture && texture.wrapMode != TextureWrapMode.Clamp)
                texture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
