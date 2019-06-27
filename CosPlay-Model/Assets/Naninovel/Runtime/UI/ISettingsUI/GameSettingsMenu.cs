// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class GameSettingsMenu : ScriptableUIBehaviour, ISettingsUI
    {
        private InputManager inputManager;

        public Task InitializeAsync () => Task.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();

            inputManager = Engine.GetService<InputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            inputManager.RemoveBlockingUI(this);
        }
    }
}
