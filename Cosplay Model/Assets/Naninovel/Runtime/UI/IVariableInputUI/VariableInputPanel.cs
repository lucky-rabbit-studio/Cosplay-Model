// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class VariableInputPanel : ScriptableUIBehaviour, IVariableInputUI
    {
        [SerializeField] private InputField inputField = default;
        [SerializeField] private Text summaryText = default;
        [SerializeField] private Button submitButton = default;

        private NovelScriptPlayer Player => Engine.GetService<NovelScriptPlayer>();
        private CustomVariableManager VariableMngr => Engine.GetService<CustomVariableManager>();
        private string variableName;
        private bool playOnSubmit;

        public Task InitializeAsync () => Task.CompletedTask;

        public void Show (string variableName, string summary, bool playOnSubmit)
        {
            this.variableName = variableName;
            this.playOnSubmit = playOnSubmit;
            summaryText.text = summary ?? string.Empty;
            summaryText.gameObject.SetActive(!string.IsNullOrWhiteSpace(summary));

            Show();
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(inputField, summaryText, submitButton);

            submitButton.interactable = false;
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            submitButton.onClick.AddListener(HandleSubmit);
            inputField.onValueChanged.AddListener(HandleInputChanged);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            submitButton.onClick.RemoveListener(HandleSubmit);
            inputField.onValueChanged.RemoveListener(HandleInputChanged);
        }

        private void HandleInputChanged (string text)
        {
            submitButton.interactable = !string.IsNullOrWhiteSpace(text);
        }

        private async void HandleSubmit ()
        {
            VariableMngr.SetVariableValue(variableName, inputField.text);

            if (playOnSubmit)
            {
                // Attempt to select and play next action.
                await Player.SelectNext();
                Player.Play();
            }

            Hide();
        }
    }
}
