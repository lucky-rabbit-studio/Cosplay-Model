// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.Video;

namespace Naninovel
{
    /// <summary>
    /// Handles movie playing.
    /// </summary>
    [InitializeAtRuntime]
    public class MoviePlayer : IEngineService
    {
        public event Action OnMoviePlay;
        public event Action OnMovieStop;
        public event Action<Texture> OnMovieTextureReady;

        public bool IsPlaying => playCTS != null && !playCTS.IsCancellationRequested;
        public float FadeDuration => config.FadeDuration;
        public Texture2D FadeTexture { get; private set; }
        public bool PlayIntroMovie => config.PlayIntroMovie;
        public string IntroMovieName => config.IntroMovieName;

        protected VideoPlayer Player { get; private set; }

        private const string defaultFadeTextureResourcesPath = "Naninovel/Textures/BlackTexture";

        private readonly MoviesConfiguration config;
        private InputManager inputManager;
        private ResourceProviderManager providerManager;
        private LocalizationManager localeManager;
        private LocalizableResourceLoader<VideoClip> videoLoader;
        private CancellationTokenSource playCTS;
        private WaitForEndOfFrame waitForEndOfFrame;
        private WaitForSeconds waitForFade;
        private string playedMovieName;

        public MoviePlayer (MoviesConfiguration config, ResourceProviderManager providerManager, LocalizationManager localeManager, InputManager inputManager)
        {
            this.config = config;
            this.providerManager = providerManager;
            this.localeManager = localeManager;
            this.inputManager = inputManager;
            waitForEndOfFrame = new WaitForEndOfFrame();
            waitForFade = new WaitForSeconds(config.FadeDuration);

            FadeTexture = ObjectUtils.IsValid(config.CustomFadeTexture) ? config.CustomFadeTexture : Resources.Load<Texture2D>(defaultFadeTextureResourcesPath);
        }

        public Task InitializeServiceAsync ()
        {
            videoLoader = new LocalizableResourceLoader<VideoClip>(config.LoaderConfiguration, providerManager, localeManager);

            Player = Engine.CreateObject<VideoPlayer>(nameof(MoviePlayer));
            Player.playOnAwake = false;
            Player.skipOnDrop = config.SkipFrames;
            #if UNITY_WEBGL && !UNITY_EDITOR
            Player.source = VideoSource.Url;
            #else
            Player.source = VideoSource.VideoClip;
            #endif
            Player.aspectRatio = config.AspectRatio;
            Player.renderMode = VideoRenderMode.APIOnly;
            Player.isLooping = false;
            Player.audioOutputMode = VideoAudioOutputMode.Direct;
            Player.loopPointReached += HandleLoopPointReached;

            inputManager.Cancel.OnStart += Stop;

            return Task.CompletedTask;
        }

        public void ResetService ()
        {
            if (IsPlaying) Stop();
            videoLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
        }

        public void DestroyService ()
        {
            if (IsPlaying) Stop();
            if (Player != null) Player.loopPointReached -= HandleLoopPointReached;
            if (inputManager != null) inputManager.Cancel.OnStart -= Stop;
            videoLoader?.GetAllLoaded()?.ForEach(r => r?.Release(this));
        }

        /// <summary>
        /// Plays a movie with the provided name; returns when the playback finishes.
        /// </summary>
        public async Task PlayAsync (string movieName)
        {
            if (IsPlaying) Stop();

            playedMovieName = movieName;
            playCTS = new CancellationTokenSource();

            OnMoviePlay?.Invoke();
            await waitForFade;

            #if UNITY_WEBGL && !UNITY_EDITOR
            Player.url = PathUtils.Combine(Application.streamingAssetsPath, videoLoader.BuildFullPath(movieName)) + ".mp4";
            #else
            var videoClipResource = await videoLoader.LoadAsync(movieName);
            if (!videoClipResource.IsValid) { Debug.LogError($"Failed to load `{movieName}` movie."); Stop(); return; }
            Player.clip = videoClipResource;
            videoClipResource.Hold(this);
            #endif

            Player.Prepare();
            while (!Player.isPrepared) await waitForEndOfFrame;
            OnMovieTextureReady?.Invoke(Player.texture);

            Player.Play();
            while (IsPlaying) await waitForEndOfFrame;
        }

        public void Stop ()
        {
            Player?.Stop();
            playCTS?.Cancel();
            playCTS?.Dispose();
            playCTS = null;

            videoLoader?.GetLoadedOrNull(playedMovieName)?.Release(this);
            playedMovieName = null;

            OnMovieStop?.Invoke();
        }

        #if UNITY_WEBGL && !UNITY_EDITOR
        public Task HoldResourcesAsync (object holder, string movieName) => Task.CompletedTask;
        public void ReleaseResources (object holder, string movieName) {}
        #else
        public async Task HoldResourcesAsync (object holder, string movieName)
        {
            var resource = await videoLoader.LoadAsync(movieName);
            if (resource.IsValid)
                resource.Hold(holder);
        }

        public void ReleaseResources (object holder, string movieName)
        {
            if (!videoLoader.IsLoaded(movieName)) return;
            var resource = videoLoader.GetLoadedOrNull(movieName);
            resource.Release(holder);
        }
        #endif

        private void HandleLoopPointReached (VideoPlayer source) => Stop();
    }
}
