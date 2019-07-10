// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class BacklogCloseButton : ScriptableLabeledButton
    {
        [ManagedText("UIBacklog")]
        public static string LabelText = "CLOSE";

        private BacklogPanel backlogPanel;

        protected override void Awake ()
        {
            base.Awake();

            backlogPanel = GetComponentInParent<BacklogPanel>();
            UIComponent.Label.text = LabelText;
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            var localeManager = Engine.GetService<LocalizationManager>();
            if (localeManager != null)
                localeManager.OnLocaleChanged += HandleLocaleChanged;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            var localeManager = Engine.GetService<LocalizationManager>();
            if (localeManager != null)
                localeManager.OnLocaleChanged -= HandleLocaleChanged;
        }

        protected override void OnButtonClick () => backlogPanel.Hide();

        private void HandleLocaleChanged (string locale) => UIComponent.Label.text = LabelText;
    }
}
