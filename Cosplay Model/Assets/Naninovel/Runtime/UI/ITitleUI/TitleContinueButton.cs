// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TitleContinueButton : ScriptableButton
    {
        [ManagedText("UITitleMenu")]
        public readonly static string LabelText = "CONTINUE";

        private StateManager gameState;
        private UIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<StateManager>();
            uiManager = Engine.GetService<UIManager>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void Start ()
        {
            base.Start();

            ControlInteractability();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            gameState.GameStateSlotManager.OnSaved += ControlInteractability;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            gameState.GameStateSlotManager.OnSaved -= ControlInteractability;
        }

        protected override void OnButtonClick ()
        {
            var saveLoadUI = uiManager.GetUI<ISaveLoadUI>();
            if (saveLoadUI is null) return;

            var lastLoadMode = saveLoadUI.GetLastLoadMode();
            saveLoadUI.PresentationMode = lastLoadMode;
            saveLoadUI.Show();
        }

        private void ControlInteractability () => UIComponent.interactable = gameState.AnyGameSaveExists;
    }
}
