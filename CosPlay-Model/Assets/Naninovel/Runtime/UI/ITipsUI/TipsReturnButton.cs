// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TipsReturnButton : ScriptableButton
    {
        [ManagedText("UITips")]
        public static string LabelText = "RETURN";

        private ITipsUI tipsUI;

        protected override void Awake ()
        {
            base.Awake();

            tipsUI = GetComponentInParent<ITipsUI>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
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

        protected override void OnButtonClick () => tipsUI.Hide();

        private void HandleLocaleChanged (string locale) => UIComponent.GetComponentInChildren<Text>().text = LabelText;
    }
}
