// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class ControlPanelTitleButton : ScriptableButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "TITLE";
        [ManagedText("UIControlPanel")]
        public readonly static string ConfirmationMessage = "Are you sure you want to quit to the title screen?\nAny unsaved game progress will be lost.";

        private StateManager gameState;
        private UIManager uiManager;
        private IConfirmationUI confirmationUI;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<StateManager>();
            uiManager = Engine.GetService<UIManager>();
            confirmationUI = uiManager.GetUI<IConfirmationUI>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void OnButtonClick () => ExitToTitleAsync();

        private async void ExitToTitleAsync ()
        {
            if (!await confirmationUI.ConfirmAsync(ConfirmationMessage)) return;

            await gameState.LoadDefaultEngineStateAsync();
            uiManager.GetUI<ITitleUI>()?.Show();
        }
    } 
}
