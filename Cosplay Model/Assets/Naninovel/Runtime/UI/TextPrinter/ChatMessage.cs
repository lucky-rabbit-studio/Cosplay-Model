// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ChatMessage : ScriptableUIBehaviour
    {
        public string PrintedText { get => printedText.text; set => printedText.text = value; }
        public Color MessageColor { get => messageFrameImage.color; set => messageFrameImage.color = value; }
        public string ActorNameText { get => actorNamePanel.Text; set => actorNamePanel.Text = value; }
        public Color ActorNameTextColor { get => actorNamePanel.TextColor; set => actorNamePanel.TextColor = value; }
        public Texture AvatarTexture { get => avatarImage.texture; set { avatarImage.texture = value; avatarImage.gameObject.SetActive(value); } }

        [SerializeField] private ActorNamePanel actorNamePanel = default;
        [SerializeField] private Text printedText = default;
        [SerializeField] private Image messageFrameImage = default;
        [SerializeField] private RawImage avatarImage = default;

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(actorNamePanel, printedText, messageFrameImage, avatarImage);
        }
    }
}
