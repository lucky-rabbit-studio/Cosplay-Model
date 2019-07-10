// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class BacklogMessage : ScriptableUIBehaviour
    {
        [SerializeField] private Text messageText = default;
        [SerializeField] private Text actorNameText = default;
        [SerializeField] private Button playVoiceButton = default;

        private List<string> voiceClipNames = new List<string>();
        private AudioManager audioManager;

        public void SetMessage (string message, string actorName)
        {
            messageText.text = message;
            if (string.IsNullOrWhiteSpace(actorName))
                actorNameText.gameObject.SetActive(false);
            else actorNameText.text = actorName;
        }

        public void AppendMessage (string message)
        {
            messageText.text += message;
        }

        public async void AddVoiceClipName (string voiceClipName)
        {
            if (string.IsNullOrWhiteSpace(voiceClipName)) return;
            if (!await audioManager.VoiceExistsAsync(voiceClipName)) return;

            voiceClipNames.Add(voiceClipName);
            playVoiceButton.gameObject.SetActive(true);
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(messageText, actorNameText, playVoiceButton);
            audioManager = Engine.GetService<AudioManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();
            playVoiceButton.onClick.AddListener(HandlePlayVoiceButtonClicked);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            playVoiceButton.onClick.RemoveListener(HandlePlayVoiceButtonClicked);
        }

        private async void HandlePlayVoiceButtonClicked ()
        {
            playVoiceButton.interactable = false;
            await audioManager.PlayVoiceSequenceAsync(voiceClipNames);
            playVoiceButton.interactable = true;
        }
    }
}
