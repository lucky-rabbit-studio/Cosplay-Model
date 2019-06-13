// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class ControlPanelLogButton : ScriptableLabeledButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "LOG";

        private UIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<UIManager>();
            UIComponent.Label.text = LabelText;
        }

        protected override void OnButtonClick () => uiManager.GetUI<IBacklogUI>()?.Show();
    } 
}
