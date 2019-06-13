// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TitleNewGameButton : ScriptableButton
    {
        [ManagedText("UITitleMenu")]
        public readonly static string LabelText = "NEW GAME";

        private string startScriptName;
        private TitleMenu titleMenu;
        private NovelScriptPlayer player;
        private StateManager stateManager;

        protected override void Awake ()
        {
            base.Awake();

            startScriptName = Engine.GetService<NovelScriptManager>()?.StartGameScriptName;
            titleMenu = GetComponentInParent<TitleMenu>();
            player = Engine.GetService<NovelScriptPlayer>();
            stateManager = Engine.GetService<StateManager>();
            Debug.Assert(titleMenu && player != null);

            UIComponent.GetComponentInChildren<Text>().text = LabelText;
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
            await stateManager.LoadDefaultEngineStateAsync(() => player.PreloadAndPlayAsync(startScriptName));
        }
    }
}
