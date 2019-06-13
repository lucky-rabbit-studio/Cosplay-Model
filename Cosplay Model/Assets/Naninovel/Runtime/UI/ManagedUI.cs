// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// A minimal implementation of <see cref="IManagedUI"/>.
    /// Can be used to create custom user managed UI prefabs.
    /// </summary>
    public class ManagedUI : ScriptableUIBehaviour, IManagedUI
    {
        [System.Serializable]
        private class GameState { public bool IsVisible; }

        [Tooltip("Whether to automatically hide the UI when loading the game.")]
        [SerializeField] private bool hideOnGameLoad = true;
        [Tooltip("Whether to preserve visibility of the UI when saving/loading the game.")]
        [SerializeField] private bool saveVisibilityState = true;

        public virtual Task InitializeAsync () => Task.CompletedTask;

        private StateManager stateManager;

        protected override void Awake ()
        {
            base.Awake();

            stateManager = Engine.GetService<StateManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (hideOnGameLoad)
                stateManager.OnLoadStarted += Hide;

            if (saveVisibilityState)
            {
                stateManager.OnGameSaveStarted += HandleGameSaveStarted;
                stateManager.OnGameLoadFinished += HandleGameLoadFinished;
            }
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (hideOnGameLoad && stateManager != null)
                stateManager.OnLoadStarted -= Hide;

            if (saveVisibilityState && stateManager != null)
            {
                stateManager.OnGameSaveStarted -= HandleGameSaveStarted;
                stateManager.OnGameLoadFinished -= HandleGameLoadFinished;
            }
        }

        protected virtual void HandleGameSaveStarted (GameSaveLoadArgs args)
        {
            var state = new GameState() {
                IsVisible = IsVisible
            };
            args.StateMap.SerializeObject(state);
        }

        protected virtual void HandleGameLoadFinished (GameSaveLoadArgs args)
        {
            var state = args.StateMap.DeserializeObject<GameState>();
            if (state is null) return;
            IsVisible = state.IsVisible;
        }
    }
}
