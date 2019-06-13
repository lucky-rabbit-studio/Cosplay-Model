// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    public class ConfirmationCancelButton : ScriptableLabeledButton
    {
        [ManagedText("UIConfirmationDialogue")]
        public readonly static string LabelText = "NO";

        protected override void Awake ()
        {
            base.Awake();

            UIComponent.Label.text = LabelText;
        }
    }
}
