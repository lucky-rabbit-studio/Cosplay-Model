// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.UI
{
    public class LoadingPanel : ScriptableUIBehaviour, ILoadingUI
    {
        private StateManager stateManager;
        private InputManager inputManager;

        public Task InitializeAsync () => Task.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();

            stateManager = Engine.GetService<StateManager>();
            inputManager = Engine.GetService<InputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.OnLoadStarted += Show;
            stateManager.OnLoadFinished += Hide;
            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            stateManager.OnLoadStarted -= Show;
            stateManager.OnLoadFinished -= Hide;
            inputManager.RemoveBlockingUI(this);
        }
    }
}
