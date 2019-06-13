// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class ControlPanelLoadButton : ScriptableButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "LOAD";

        private UIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<UIManager>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void OnButtonClick ()
        {
            var saveLoadUI = uiManager.GetUI<ISaveLoadUI>();
            if (saveLoadUI is null) return;

            saveLoadUI.PresentationMode = SaveLoadUIPresentationMode.Load;
            saveLoadUI.Show();
        }
    } 
}
