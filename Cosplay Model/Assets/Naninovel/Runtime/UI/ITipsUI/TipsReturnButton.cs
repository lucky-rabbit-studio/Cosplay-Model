// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TipsReturnButton : ScriptableButton
    {
        [ManagedText("UITips")]
        public readonly static string LabelText = "RETURN";

        private ITipsUI tipsUI;

        protected override void Awake ()
        {
            base.Awake();

            tipsUI = GetComponentInParent<ITipsUI>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void OnButtonClick () => tipsUI.Hide();
    }
}
