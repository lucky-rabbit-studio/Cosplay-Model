// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class MovieUI : ScriptableUIBehaviour, IMovieUI
    {
        [SerializeField] private RawImage movieImage = default;
        [SerializeField] private RawImage fadeImage = default;

        private MoviePlayer moviePlayer;
        private InputManager inputManager;

        public Task InitializeAsync () => Task.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(movieImage, fadeImage);
            moviePlayer = Engine.GetService<MoviePlayer>();
            inputManager = Engine.GetService<InputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            moviePlayer.OnMoviePlay += HandleMoviePlay;
            moviePlayer.OnMovieStop += HandleMovieStop;
            moviePlayer.OnMovieTextureReady += HandleMovieTextureReady;
            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            moviePlayer.OnMoviePlay -= HandleMoviePlay;
            moviePlayer.OnMovieStop -= HandleMovieStop;
            moviePlayer.OnMovieTextureReady -= HandleMovieTextureReady;
            inputManager.RemoveBlockingUI(this);
        }

        private async void HandleMoviePlay ()
        {
            fadeImage.texture = moviePlayer.FadeTexture;
            movieImage.texture = moviePlayer.FadeTexture;
            await SetIsVisibleAsync(true, moviePlayer.FadeDuration);
        }

        private void HandleMovieTextureReady (Texture texture)
        {
            movieImage.texture = texture;
        }

        private async void HandleMovieStop ()
        {
            movieImage.texture = moviePlayer.FadeTexture;
            await SetIsVisibleAsync(false, moviePlayer.FadeDuration);
        }
    }
}
