// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class TitleExitButton : ScriptableButton
    {
        protected override void OnButtonClick ()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                Application.OpenURL("about:blank");
            else Application.Quit();
        }
    }
}
