// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsReturnButton : ScriptableButton
    {
        private GameSettingsMenu settingsMenu;
        private StateManager settingsManager;

        protected override void Awake ()
        {
            base.Awake();

            settingsMenu = GetComponentInParent<GameSettingsMenu>();
            settingsManager = Engine.GetService<StateManager>();
        }

        protected override void OnButtonClick () => ApplySettingsAsync();

        private async void ApplySettingsAsync ()
        {
            settingsMenu.SetIsInteractable(false);
            await settingsManager.SaveSettingsAsync();
            settingsMenu.SetIsInteractable(true);
            settingsMenu.Hide();
        }
    }
}
