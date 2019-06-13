// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    public class ConfirmationConfirmButton : ScriptableLabeledButton
    {
        [ManagedText("UIConfirmationDialogue")]
        public readonly static string LabelText = "YES";

        protected override void Awake ()
        {
            base.Awake();

            UIComponent.Label.text = LabelText;
        }
    }
}
