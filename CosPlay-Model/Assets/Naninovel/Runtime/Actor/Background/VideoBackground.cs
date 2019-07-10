// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.Video;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="VideoPlayer"/> to represent an actor.
    /// </summary>
    public class VideoBackground : MonoBehaviourActor, IBackgroundActor
    {
        private class VideoData { public VideoPlayer Player; public RenderTexture RenderTexture; }

        public override string Appearance { get => appearance; set => SetAppearance(value); }
        public override bool IsVisible { get => isVisible; set => SetVisibility(value); }

        protected TransitionalSpriteRenderer SpriteRenderer { get; }

        private string appearance;
        private bool isVisible;
        private LocalizableResourceLoader<VideoClip> videoLoader;
        private static bool sharedResourcesInitialized;
        private static int sharedRefCounter;
        private static RenderTextureDescriptor renderTextureDescriptor;
        private static LiteralMap<VideoData> videoDataMap;
        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

        public VideoBackground (string id, BackgroundMetadata metadata)
            : base(id, metadata)
        {
            InitializeSharedResources();
            sharedRefCounter++;

            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var localeMngr = Engine.GetService<LocalizationManager>();
            videoLoader = new LocalizableResourceLoader<VideoClip>(
                providerMngr.GetProviderList(metadata.LoaderConfiguration.ProviderTypes), 
                localeMngr, $"{metadata.LoaderConfiguration.PathPrefix}/{id}");

            SpriteRenderer = GameObject.AddComponent<TransitionalSpriteRenderer>();
            SpriteRenderer.Pivot = metadata.Pivot;
            SpriteRenderer.PixelsPerUnit = metadata.PixelsPerUnit;

            SetVisibility(false);
        }

        public override async Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            this.appearance = appearance;

            if (string.IsNullOrEmpty(appearance)) return;

            var videoData = await GetOrLoadVideoDataAsync(appearance);
            if (!videoData.Player.isPlaying) videoData.Player.Play();
            await SpriteRenderer.TransitionToAsync(videoData.RenderTexture, duration, easingType);
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

            await GetOrLoadVideoDataAsync(appearance);

            // Releasing is done in Dispose().
            videoLoader.GetLoadedOrNull(appearance)?.Hold(this);
        }

        public override void Dispose ()
        {
            base.Dispose();

            videoLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
            sharedRefCounter--;
            DestroySharedResources();
        }

        protected virtual void SetAppearance (string appearance) => ChangeAppearanceAsync(appearance, 0).WrapAsync();

        protected virtual void SetVisibility (bool isVisible) => ChangeVisibilityAsync(isVisible, 0).WrapAsync();

        protected override Color GetBehaviourTintColor () => Color.white;

        protected override void SetBehaviourTintColor (Color tintColor) { }

        private async Task<VideoData> GetOrLoadVideoDataAsync (string videoName)
        {
            if (videoDataMap.ContainsKey(videoName)) return videoDataMap[videoName];

            var renderTexture = new RenderTexture(renderTextureDescriptor);
            var videoPlayer = Engine.CreateObject<VideoPlayer>(videoName);

            #if UNITY_WEBGL && !UNITY_EDITOR
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = PathUtils.Combine(Application.streamingAssetsPath, videoLoader.BuildFullPath(videoName)) + ".mp4";
            await new WaitForEndOfFrame();
            #else
            var videoClip = await videoLoader.LoadAsync(videoName);
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
            #endif

            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared) await waitForEndOfFrame;

            var sceneData = new VideoData { Player = videoPlayer, RenderTexture = renderTexture };
            videoDataMap[videoName] = sceneData;

            return sceneData;
        }

        private static void InitializeSharedResources ()
        {
            if (sharedResourcesInitialized) return;

            var camera = Engine.GetService<CameraManager>();
            renderTextureDescriptor = new RenderTextureDescriptor((int)camera.ReferenceResolution.x, (int)camera.ReferenceResolution.y, RenderTextureFormat.Default);
            videoDataMap = new LiteralMap<VideoData>();
            sharedResourcesInitialized = true;
        }

        private static void DestroySharedResources ()
        {
            if (sharedRefCounter > 0) return;

            foreach (var videoData in videoDataMap.Values)
            {
                videoData.Player.Stop();
                if (Application.isPlaying)
                {
                    Object.Destroy(videoData.Player.gameObject);
                    Object.Destroy(videoData.RenderTexture);
                }
                else
                {
                    Object.DestroyImmediate(videoData.Player.gameObject);
                    Object.DestroyImmediate(videoData.RenderTexture);
                }
            }

            sharedResourcesInitialized = false;
        }
    }
}
