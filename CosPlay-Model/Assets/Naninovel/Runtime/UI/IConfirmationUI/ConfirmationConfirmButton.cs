// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    public class ConfirmationConfirmButton : ScriptableLabeledButton
    {
        [ManagedText("UIConfirmationDialogue")]
        public static string LabelText = "YES";

        protected override void Awake ()
        {
            base.Awake();

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

        private void HandleLocaleChanged (string locale) => UIComponent.Label.text = LabelText;
    }
}
