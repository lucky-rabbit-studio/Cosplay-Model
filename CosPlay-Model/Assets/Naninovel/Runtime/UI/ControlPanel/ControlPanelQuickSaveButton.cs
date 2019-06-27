// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class ControlPanelQuickSaveButton : ScriptableButton
    {
        private StateManager gameState;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<StateManager>();
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
