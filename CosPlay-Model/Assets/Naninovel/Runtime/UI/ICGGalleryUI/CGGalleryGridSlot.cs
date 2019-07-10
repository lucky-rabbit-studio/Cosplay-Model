// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class CGGalleryGridSlot : ScriptableGridSlot
    {
        public class Constructor : Constructor<CGGalleryGridSlot>
        {
            public Constructor (CGGalleryGridSlot prototype, string unlockableId, string textureLocalPath, 
                LocalizableResourceLoader<Texture2D> cgTextureLoader, OnClicked onClicked) : base(prototype, unlockableId, onClicked)
            {
                ConstructedSlot.textureLoader = cgTextureLoader;
                ConstructedSlot.textureLocalPath = textureLocalPath;
                ConstructedSlot.thumbnailImage.texture = ConstructedSlot.loadingTexture;
            }
        }

        public string UnlockableId => Id;

        [SerializeField] private RawImage thumbnailImage = null;
        [SerializeField] private Texture2D lockedTexture = default;
        [SerializeField] private Texture2D loadingTexture = default;

        private string textureLocalPath;
        private UnlockableManager unlockableManager;
        private LocalizableResourceLoader<Texture2D> textureLoader;

        public async Task<Texture2D> LoadCGTextureAsync ()
        {
            Texture2D cgTexture;

            if (textureLoader.IsLoaded(textureLocalPath))
                cgTexture = textureLoader.GetLoadedOrNull(textureLocalPath);
            else
            {
                thumbnailImage.texture = loadingTexture;
                cgTexture = await textureLoader.LoadAsync(textureLocalPath);
            }

            thumbnailImage.texture = unlockableManager.ItemUnlocked(UnlockableId) ? cgTexture : lockedTexture;

            return cgTexture;
        }

        public void UnloadCGTexture () => textureLoader.Unload(textureLocalPath);

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(thumbnailImage, lockedTexture);

            unlockableManager = Engine.GetService<UnlockableManager>();
        }
    }
}
