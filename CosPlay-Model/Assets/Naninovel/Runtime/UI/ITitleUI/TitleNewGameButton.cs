// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class TitleNewGameButton : ScriptableButton
    {
        private string startScriptName;
        private TitleMenu titleMenu;
        private ScriptPlayer player;
        private StateManager stateManager;

        protected override void Awake ()
        {
            base.Awake();

            startScriptName = Engine.GetService<ScriptManager>()?.StartGameScriptName;
            titleMenu = GetComponentInParent<TitleMenu>();
            player = Engine.GetService<ScriptPlayer>();
            stateManager = Engine.GetService<StateManager>();
            Debug.Assert(titleMenu && player != null);
        }

        protected override void Start ()
        {
            base.Start();

            if (string.IsNullOrEmpty(startScriptName))
                UIComponent.interactable = false;
        }

        protected override void OnButtonClick ()
        {
            if (string.IsNullOrEmpty(startScriptName))
            {
                Debug.LogError("Can't start new game: please specify start script name in the settings.");
                return;
            }

            titleMenu.Hide();
            StartNewGameAsync();
        }

        private async void StartNewGameAsync ()
        {
            Engine.GetService<CustomVariableManager>()?.ResetLocalVariables();
            await stateManager.ResetStateAsync(() => player.PreloadAndPlayAsync(startScriptName));
        }
    }
}
