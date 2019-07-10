// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    /// <summary>
    /// A <see cref="UITextPrinterPanel"/> implementation for a chat-style printer.
    /// </summary>
    public class ChatPrinterPanel : UITextPrinterPanel
    {
        public override string PrintedText { get; set; }
        public override string ActorNameText { get; set; }

        protected string LastAuthorId { get; private set; }
        protected CharacterMetadata LastAuthorMeta { get; private set; }

        [SerializeField] private ScrollRect scrollRect = default;
        [SerializeField] private RectTransform messagesContainer = default;
        [SerializeField] private ChatMessage messagePrototype = default;
        [SerializeField] private ScriptableUIBehaviour inputIndicator = default;
        [SerializeField] private float revealDelayModifier = 3f;
        [SerializeField] private float printDotDelay = .5f;

        private Stack<ChatMessage> messagesStack = new Stack<ChatMessage>();

        public override IEnumerator RevealPrintedTextOverTime (CancellationToken cancellationToken, float revealDelay)
        {
            var message = AddMessage();

            if (revealDelay > 0)
            {
                var revealFinishTime = Time.time + PrintedText.Count(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)) * revealDelay * revealDelayModifier;
                var lastPrintDotTime = 0f;
                while (revealFinishTime > Time.time)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    // Print dots while waiting.
                    if (Time.time >= lastPrintDotTime + printDotDelay)
                    {
                        lastPrintDotTime = Time.time;
                        message.PrintedText = message.PrintedText.Length >= 9 ? string.Empty : message.PrintedText + " . ";
                    }

                    yield return null;
                }
            }

            message.PrintedText = PrintedText;
            ScrollToBottom();
        }

        public override void RevealPrintedText ()
        {
            if (messagesStack.Count == 0)
            {
                if (string.IsNullOrEmpty(PrintedText)) return;
                AddMessage();
            }

            var lastMessage = messagesStack.Peek();
            if (lastMessage.PrintedText != PrintedText)
                lastMessage = AddMessage();

            lastMessage.PrintedText = PrintedText;
            lastMessage.IsVisible = true;
            ScrollToBottom();
        }

        public override void HidePrintedText ()
        {
            // Usually called to clear the printer, so removing all messages here.
            DestroyAllMessages();
        }

        public override void SetWaitForInputIndicatorVisible (bool isVisible)
        {
            if (isVisible) inputIndicator.Show();
            else inputIndicator.Hide();
        }

        public override void OnAuthorChanged (string authorId, CharacterMetadata authorMeta)
        {
            LastAuthorId = authorId;
            LastAuthorMeta = authorMeta;
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(scrollRect, messagesContainer, messagePrototype, inputIndicator);
        }

        private ChatMessage AddMessage ()
        {
            var message = Instantiate(messagePrototype);
            message.transform.SetParent(messagesContainer, false);
            message.ActorNameText = ActorNameText;

            if (LastAuthorMeta != null && LastAuthorMeta.UseCharacterColor)
            {
                message.MessageColor = LastAuthorMeta.MessageColor;
                message.ActorNameTextColor = LastAuthorMeta.NameColor;
            }

            message.AvatarTexture = CharacterManager.GetAvatarTextureFor(LastAuthorId);

            message.Show();
            messagesStack.Push(message);
            ScrollToBottom();
            return message;
        }

        private void DestroyAllMessages ()
        {
            for (int i = 0; i < messagesStack.Count; i++)
            {
                var message = messagesStack.Pop();
                Destroy(message);
            }
        }

        private async void ScrollToBottom ()
        {
            await new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0;
        }
    } 
}
