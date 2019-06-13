// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TitleExitButton : ScriptableButton
    {
        [ManagedText("UITitleMenu")]
        public readonly static string LabelText = "EXIT";

        protected override void Awake ()
        {
            base.Awake();

            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void OnButtonClick ()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                Application.OpenURL("about:blank");
            else Application.Quit();
        }
    }
}
