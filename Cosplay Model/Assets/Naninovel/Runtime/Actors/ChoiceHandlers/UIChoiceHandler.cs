// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using Naninovel.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IChoiceHandlerActor"/> implementation using <see cref="UI.ChoiceHandlerPanel"/> to represent an actor.
    /// </summary>
    public class UIChoiceHandler : MonoBehaviourActor, IChoiceHandlerActor
    {
        public override string Appearance { get; set; }
        public override bool IsVisible { get => HandlerPanel.IsVisible; set => HandlerPanel.IsVisible = value; }
        public bool IsHandlerActive { get => isHandlerActive; set => SetIsHandlerActive(value); }
        public IEnumerable<ChoiceState> Choices => choices;

        protected ChoiceHandlerPanel HandlerPanel { get; private set; }

        private ChoiceHandlerMetadata metadata;
        private bool isHandlerActive = false;
        private List<ChoiceState> choices = new List<ChoiceState>();

        public UIChoiceHandler (string id, ChoiceHandlerMetadata metadata)
            : base(id, metadata)
        {
            this.metadata = metadata;
        }

        public override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var prefabResource = await metadata.LoaderConfiguration.CreateFor<GameObject>(providerMngr).LoadAsync(Id);
            if (!prefabResource.IsValid)
            {
                Debug.LogError($"Failed to load `{Id}` choice handler resource object. Make sure the handler is correctly configured.");
                return;
            }

            var uiMngr = Engine.GetService<UIManager>();
            HandlerPanel = uiMngr.InstantiateUIPrefab(prefabResource.Object) as ChoiceHandlerPanel;
            HandlerPanel.OnChoice += HandleChoice;
            HandlerPanel.transform.SetParent(Transform);

            await HandlerPanel.InitializeAsync();

            IsVisible = false;
        }

        public override Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            throw new System.NotImplementedException();
        }

        public override async Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            await HandlerPanel?.SetIsVisibleAsync(isVisible, duration);
        }

        public virtual void AddChoice (ChoiceState choice)
        {
            HandlerPanel.Show();
            choices.Add(choice);
            HandlerPanel.AddChoiceButton(choice);
        }

        public virtual void RemoveChoice (string id)
        {
            choices.RemoveAll(c => c.Id == id);
            HandlerPanel.RemoveChoiceButton(id);
            if (choices.Count == 0) HandlerPanel.Hide();
        }

        public ChoiceState GetChoice (string id) => choices.FirstOrDefault(c => c.Id == id);

        public virtual void SetIsHandlerActive (bool value)
        {
            isHandlerActive = value;
            if (value) HandlerPanel.Show();
            else HandlerPanel.Hide();
        }

        public override Task PreloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        public override Task UnloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        protected override Color GetBehaviourTintColor () => Color.white;

        protected override void SetBehaviourTintColor (Color tintColor) { }

        protected async void HandleChoice (ChoiceState state)
        {
            if (!IsHandlerActive) return;

            HandlerPanel?.RemoveAllChoiceButtons();
            choices.Clear();
            IsHandlerActive = false;

            if (!string.IsNullOrEmpty(state.SetExpression))
            {
                var setAction = new SetCustomVariable { Expression = state.SetExpression };
                if (state.UndoData != null)
                    state.UndoData.SetAction = setAction;
                await setAction.ExecuteAsync();
            }

            if (string.IsNullOrWhiteSpace(state.GotoScript) && string.IsNullOrWhiteSpace(state.GotoLabel))
            {
                // When no goto param specified -- attempt to select and play next action.
                var player = Engine.GetService<NovelScriptPlayer>();
                await player.SelectNext();
                player.Play();
            }
            else await new Actions.Goto { Path = new Named<string>(state.GotoScript, state.GotoLabel) }.ExecuteAsync();
        }
    } 
}
