// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class BacklogCloseButton : ScriptableLabeledButton
    {
        [ManagedText("UIBacklog")]
        public readonly static string LabelText = "CLOSE";

        private BacklogPanel backlogPanel;

        protected override void Awake ()
        {
            base.Awake();

            backlogPanel = GetComponentInParent<BacklogPanel>();
            UIComponent.Label.text = LabelText;
        }

        protected override void OnButtonClick () => backlogPanel.Hide();
    }
}
