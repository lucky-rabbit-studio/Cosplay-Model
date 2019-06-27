// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class ConfirmationPanel : ScriptableUIBehaviour, IConfirmationUI
    {
        [SerializeField] private Text messageText = default;
        [SerializeField] private ScriptableLabeledButton confirmButton = default;
        [SerializeField] private ScriptableLabeledButton cancelButton = default;

        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private InputManager inputManager;
        private bool? userConfirmed;

        public Task InitializeAsync () => Task.CompletedTask;

        public async Task<bool> ConfirmAsync (string message)
        {
            if (IsVisible) return false;

            messageText.text = message;

            Show();

            while (!userConfirmed.HasValue)
                await waitForEndOfFrame;

            var result = userConfirmed.Value;
            userConfirmed = null;

            Hide();

            return result;
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(messageText, confirmButton, cancelButton);

            inputManager = Engine.GetService<InputManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            inputManager.AddBlockingUI(this);
            confirmButton.OnButtonClicked += Confirm;
            cancelButton.OnButtonClicked += Cancel;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            inputManager?.RemoveBlockingUI(this);
            confirmButton.OnButtonClicked -= Confirm;
            cancelButton.OnButtonClicked -= Cancel;
        }

        private void Confirm ()
        {
            if (!IsVisible) return;
            userConfirmed = true;
        }

        private void Cancel ()
        {
            if (!IsVisible) return;
            userConfirmed = false;
        }
    }
}
