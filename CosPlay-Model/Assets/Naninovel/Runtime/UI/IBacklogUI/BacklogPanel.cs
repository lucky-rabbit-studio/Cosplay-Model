// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class BacklogPanel : ScriptableUIBehaviour, IBacklogUI
    {
        protected virtual BacklogMessage LastMessage => messageStack != null && messageStack.Count > 0 ? messageStack.Peek() : null;

        [SerializeField] private RectTransform messagesContainer = default;
        [SerializeField] private ScrollRect scrollRect = default;
        [SerializeField] private BacklogMessage messagePrefab = default;
        [Tooltip("Whether to clear the backlog when loading a game or returning to the title screen.")]
        [SerializeField] private bool clearOnLoading = true;

        private InputManager inputManager;
        private CharacterManager charManager;
        private StateManager stateManager;
        private Stack<BacklogMessage> messageStack = new Stack<BacklogMessage>();

        public Task InitializeAsync () => Task.CompletedTask;

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(messagesContainer, scrollRect, messagePrefab);

            inputManager = Engine.GetService<InputManager>();
            charManager = Engine.GetService<CharacterManager>();
            stateManager = Engine.GetService<StateManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            inputManager.AddBlockingUI(this);
            inputManager.ShowBacklog.OnStart += Show;

            if (clearOnLoading)
                stateManager.OnLoadStarted += Clear;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            inputManager.RemoveBlockingUI(this);
            inputManager.ShowBacklog.OnStart -= Show;

            if (stateManager != null)
                stateManager.OnLoadStarted -= Clear;
        }

        public void Clear ()
        {
            foreach (var message in messageStack)
                Destroy(message.gameObject);
            messageStack.Clear();
        }

        public void AddMessage (string messageText, string actorId = null, string voiceClipName = null)
        {
            var message = Instantiate(messagePrefab);
            var actorName = charManager.GetDisplayName(actorId) ?? actorId;
            message.SetMessage(messageText, actorName);
            message.transform.SetParent(messagesContainer.transform, false);
            if (!string.IsNullOrWhiteSpace(voiceClipName))
                message.AddVoiceClipName(voiceClipName);
            messageStack.Push(message);
        }

        public void AppendMessage (string message, string voiceClipName = null)
        {
            if (!LastMessage) return;
            LastMessage.AppendMessage(message);
            if (!string.IsNullOrWhiteSpace(voiceClipName))
                LastMessage.AddVoiceClipName(voiceClipName);
        }

        public override void SetIsVisible (bool isVisible)
        {
            if (isVisible) ScrollToBottom();
            base.SetIsVisible(isVisible);
        }

        public override Task SetIsVisibleAsync (bool isVisible, float? fadeTime = null)
        {
            if (isVisible) ScrollToBottom();
            return base.SetIsVisibleAsync(isVisible, fadeTime);
        }

        private async void ScrollToBottom ()
        {
            await new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0;
        }
    }
}
