// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(Button))]
    public class ChoiceHandlerButton : ScriptableButton
    {
        public string Id { get; private set; }

        private Text summaryText;

        protected override void Awake ()
        {
            base.Awake();

            summaryText = GetComponentInChildren<Text>();
            if (summaryText)
                summaryText.text = "Choice summary (not initialized)";
        }

        public virtual void Initialize (ChoiceState choiceState)
        {
            Id = choiceState.Id;

            if (summaryText)
                summaryText.text = choiceState.Summary;
        }
    } 
}
