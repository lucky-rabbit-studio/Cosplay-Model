// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TitleMenu : ScriptableUIBehaviour, ITitleUI
    {
        private ScriptPlayer player;
        private StateManager gameState;
        private InputManager inputManager;
        private string titleScriptName;

        public Task InitializeAsync () => Task.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<ScriptPlayer>();
            gameState = Engine.GetService<StateManager>();
            inputManager = Engine.GetService<InputManager>();
            titleScriptName = Configuration.LoadOrDefault<ScriptsConfiguration>()?.TitleScript;
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            player.OnPlay += Hide;
            gameState.GameStateSlotManager.OnBeforeLoad += Hide;
            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            player.OnPlay -= Hide;
            gameState.GameStateSlotManager.OnBeforeLoad -= Hide;
            inputManager.RemoveBlockingUI(this);
        }

        public override async Task SetIsVisibleAsync (bool isVisible, float? fadeTime = null)
        {
            if (isVisible && !string.IsNullOrEmpty(titleScriptName))
                await player.PreloadAndPlayAsync(titleScriptName);
            await base.SetIsVisibleAsync(isVisible, fadeTime);
        }
    }
}
