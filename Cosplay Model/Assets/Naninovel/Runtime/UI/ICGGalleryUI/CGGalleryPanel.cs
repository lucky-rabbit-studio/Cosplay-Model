// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CGGalleryPanel : ScriptableUIBehaviour, ICGGalleryUI
    {
        public int CGCount => grid.SlotCount;

        [Header("CG Setup")]
        [Tooltip("All the unlockable item IDs with the specified prefix will be considered CG items.")]
        [SerializeField] private string unlockableIdPrefix = "CG";
        [Tooltip("The spcified resource loaders will be used to retrieve the available CG slots and associated textures.")]
        [SerializeField] private ResourceLoaderConfiguration[] cgSources = new[] {
            new ResourceLoaderConfiguration { PathPrefix = $"{UnlockablesConfiguration.DefaultUnlockablesPathPrefix}/CG" },
            new ResourceLoaderConfiguration { PathPrefix = $"{BackgroundsConfiguration.DefaultBackgroundsPathPrefix}/{BackgroundManager.MainActorId}/CG" },
        };

        [Header("UI Setup")]
        [SerializeField] private ScriptableButton viewerPanel = default;
        [SerializeField] private RawImage viewerImage = default;
        [SerializeField] private CGGalleryGrid grid = default;

        private UnlockableManager unlockableManager;
        private ResourceProviderManager providerManager;
        private LocalizationManager localizationManager;
        private InputManager inputManager;

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(grid, viewerPanel, viewerImage);

            unlockableManager = Engine.GetService<UnlockableManager>();
            providerManager = Engine.GetService<ResourceProviderManager>();
            localizationManager = Engine.GetService<LocalizationManager>();
            inputManager = Engine.GetService<InputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            OnVisibilityChanged += HandleVisibilityChanged;
            viewerPanel.OnButtonClicked += viewerPanel.Hide;
            inputManager.Cancel.OnStart += viewerPanel.Hide;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            OnVisibilityChanged -= HandleVisibilityChanged;
            viewerPanel.OnButtonClicked -= viewerPanel.Hide;
            inputManager.Cancel.OnStart -= viewerPanel.Hide;
        }

        public async Task InitializeAsync ()
        {
            foreach (var loaderConfig in cgSources)
            {
                // 1. Locate all the available textures under the source path.
                var loader = new LocalizableResourceLoader<Texture2D>(loaderConfig, providerManager, localizationManager);
                var resources = await loader.LocateResourcesAsync(string.Empty);
                // 2. Iterate the textures, adding them to the grid as CG slots.
                foreach (var resource in resources)
                {
                    var textureLocalPath = loader.BuildLocalPath(resource.Path);
                    var unlockableId = $"{unlockableIdPrefix}/{textureLocalPath}";
                    if (grid.SlotExists(unlockableId)) continue;
                    grid.AddSlot(new CGGalleryGridSlot.Constructor(grid.SlotPrototype, unlockableId, textureLocalPath, loader, HandleSlotClicked).ConstructedSlot);
                }
            }
        }

        private async void HandleVisibilityChanged (bool visible)
        {
            foreach (var slot in grid.GetAllSlots())
            {
                if (visible) await slot.LoadCGTextureAsync();
                else await slot.UnloadCGTextureAsync();
            }
        }

        private async void HandleSlotClicked (string id)
        {
            var slot = grid.GetSlot(id);
            if (!unlockableManager.ItemUnlocked(slot.UnlockableId)) return;

            var cgTexture = await slot.LoadCGTextureAsync();
            viewerImage.texture = cgTexture;
            viewerPanel.Show();
        }
    }
}
