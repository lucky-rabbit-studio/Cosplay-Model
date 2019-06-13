// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a view for choosing between a set of choices.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ChoiceHandlerPanel : ScriptableUIBehaviour, IManagedUI
    {
        /// <summary>
        /// Invoked when one of active choices are choosen.
        /// </summary>
        public event Action<ChoiceState> OnChoice;

        [Tooltip("Container that will hold spawned choice buttons.")]
        [SerializeField] private RectTransform buttonsContainer = null;
        [Tooltip("Button prototype to use by default.")]
        [SerializeField] private ChoiceHandlerButton defaultButtonPrefab = null;

        private List<ChoiceHandlerButton> choiceButtons = new List<ChoiceHandlerButton>();

        public virtual Task InitializeAsync () => Task.CompletedTask;

        public virtual void AddChoiceButton (ChoiceState choice)
        {
            var choicePrefab = string.IsNullOrWhiteSpace(choice.ButtonPath) ? defaultButtonPrefab : Resources.Load<ChoiceHandlerButton>(choice.ButtonPath);
            var choiceButton = Instantiate(choicePrefab);
            choiceButton.transform.SetParent(buttonsContainer, false);
            choiceButton.Initialize(choice);
            choiceButton.OnButtonClicked += () => OnChoice?.Invoke(choice);

            if (choice.OverwriteButtonPosition)
                choiceButton.transform.localPosition = choice.ButtonPosition;

            choiceButtons.Add(choiceButton);
        }

        public virtual void RemoveChoiceButton (string id)
        {
            var buttons = choiceButtons.FindAll(c => c.Id == id);
            if (buttons is null || buttons.Count == 0) return;

            foreach (var button in buttons)
            {
                if (button) Destroy(button.gameObject);
                choiceButtons.Remove(button);
            }
        }

        public virtual void RemoveAllChoiceButtons ()
        {
            for (int i = 0; i < choiceButtons.Count; i++)
                Destroy(choiceButtons[i].gameObject);
            choiceButtons.Clear();
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(defaultButtonPrefab, buttonsContainer);
        }
    } 
}
