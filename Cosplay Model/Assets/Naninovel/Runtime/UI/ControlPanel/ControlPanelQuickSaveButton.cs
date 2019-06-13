// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class ControlPanelQuickSaveButton : ScriptableButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "Q.SAVE";

        private StateManager gameState;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<StateManager>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void OnButtonClick () => QuickSaveAsync();

        private async void QuickSaveAsync ()
        {
            UIComponent.interactable = false;
            await gameState.QuickSaveAsync();
            UIComponent.interactable = true;
        }
    } 
}
