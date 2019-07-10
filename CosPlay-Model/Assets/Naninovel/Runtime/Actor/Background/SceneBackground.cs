// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="Scene"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// The implementation currently requires scenes to be at `./Assets/Scenes` project folder; resource providers are not supported.
    /// Scenes should be added to the build settings.
    /// </remarks>
    public class SceneBackground : MonoBehaviourActor, IBackgroundActor
    {
        private class SceneData { public Scene Scene; public GameObject RootObject; public RenderTexture RenderTexture; }

        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected TransitionalSpriteRenderer SpriteRenderer { get; }

        private const string pathPrefix = "Assets/Scenes/";
        private const float rootPadding = 1000f;
        private string appearance;
        private bool isVisible;
        private static bool sharedResourcesInitialized;
        private static int sharedRefCounter;
        private static RenderTextureDescriptor renderTextureDescriptor;
        private static LiteralMap<SceneData> sceneDataMap;

        public SceneBackground (string id, BackgroundMetadata metadata) 
            : base(id, metadata)
        {
            InitializeSharedResources();
            sharedRefCounter++;

            SpriteRenderer = GameObject.AddComponent<TransitionalSpriteRenderer>();
            SpriteRenderer.Pivot = metadata.Pivot;
            SpriteRenderer.PixelsPerUnit = metadata.PixelsPerUnit;

            SetVisibility(false);
        }

        public override async Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            this.appearance = appearance;

            if (string.IsNullOrEmpty(appearance)) return;

            var scene = await GetOrLoadSceneDataAsync(appearance);
            await SpriteRenderer.TransitionToAsync(scene.RenderTexture, duration, easingType);
        }

        public async Task TransitionAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null)
        {
            if (transitionType.HasValue) SpriteRenderer.TransitionType = transitionType.Value;
            if (transitionParams.HasValue) SpriteRenderer.TransitionParams = transitionParams.Value;
            if (ObjectUtils.IsValid(customDissolveTexture)) SpriteRenderer.CustomDissolveTexture = customDissolveTexture;

            await ChangeAppearanceAsync(appearance, duration, easingType);
        }

        public override async Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            this.isVisible = isVisible;

            await SpriteRenderer.FadeToAsync(isVisible ? 1 : 0, duration, easingType);
        }

        public override async Task HoldResourcesAsync (object holder, string appearance)
        {
            if (string.IsNullOrEmpty(appearance)) return;

            await GetOrLoadSceneDataAsync(appearance);

            // Releasing is managed by Dispose().
        }

        public override void Dispose ()
        {
            base.Dispose();

            sharedRefCounter--;
            DestroySharedResources();
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible) => ChangeVisibilityAsync(isVisible, 0).WrapAsync();

        protected override Color GetBehaviourTintColor () => Color.white;

        protected override void SetBehaviourTintColor (Color tintColor) { }

        private async Task<SceneData> GetOrLoadSceneDataAsync (string sceneName)
        {
            if (sceneDataMap.ContainsKey(sceneName)) return sceneDataMap[sceneName];

            var scenePath = string.Concat(pathPrefix, sceneName, ".unity");

            // Load scene.
            await SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByPath(scenePath);
            Debug.Assert(scene.isLoaded, $"Failed loading scene `{scenePath}`. Make sure the scene is added to the build settings and located at `{pathPrefix}`.");

            // Add scene's root objects to new single root object.
            var rootObject = new GameObject(scenePath);
            SceneManager.MoveGameObjectToScene(rootObject, scene);
            foreach (var obj in scene.GetRootGameObjects())
                obj.transform.SetParent(rootObject.transform, false);

            // Move root object by padding value, so that it won't interfere with other scenes.
            var xFactor = sceneDataMap.Count + (sceneDataMap.Count.IsEven() ? 1 : 0);
            var yFactor = sceneDataMap.Count + (sceneDataMap.Count.IsEven() ? 0 : 1);
            rootObject.transform.AddPosX(rootPadding * xFactor);
            rootObject.transform.AddPosY(rootPadding * yFactor);

            // Create render texture and assign to first found camera of the scene's objects.
            var renderTexture = new RenderTexture(renderTextureDescriptor);
            var camera = rootObject.GetComponentInChildren<Camera>(false);
            Debug.Assert(camera, $"Camera component is not found in `{scenePath}` scene.");
            camera.targetTexture = renderTexture;

            // Commit shared data.
            var sceneData = new SceneData { Scene = scene, RootObject = rootObject, RenderTexture = renderTexture };
            sceneDataMap[sceneName] = sceneData;

            return sceneData;
        }

        private static void InitializeSharedResources ()
        {
            if (sharedResourcesInitialized) return;

            var camera = Engine.GetService<CameraManager>();
            renderTextureDescriptor = new RenderTextureDescriptor((int)camera.ReferenceResolution.x, (int)camera.ReferenceResolution.y, RenderTextureFormat.Default);
            sceneDataMap = new LiteralMap<SceneData>();
            sharedResourcesInitialized = true;
        }

        private static void DestroySharedResources ()
        {
            if (sharedRefCounter > 0) return;

            foreach (var sceneData in sceneDataMap.Values)
                SceneManager.UnloadSceneAsync(sceneData.Scene);
            sceneDataMap.Clear();

            sharedResourcesInitialized = false;
        }
    }
}
