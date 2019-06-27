// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class NavigatorPlaytButton : ScriptableButton, IPointerEnterHandler, IPointerExitHandler
    {
        private Text labelText;
        private ScriptNavigatorPanel navigator;
        private Script script;
        private ScriptPlayer player;
        private StateManager stateManager;
        private bool isInitialized;

        protected override void Awake ()
        {
            base.Awake();

            labelText = GetComponentInChildren<Text>();
            labelText.text = "Loading...";
            UIComponent.interactable = false;

            stateManager = Engine.GetService<StateManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.GameStateSlotManager.OnBeforeLoad += ControlInteractability;
            stateManager.GameStateSlotManager.OnLoaded += ControlInteractability;
            stateManager.GameStateSlotManager.OnBeforeSave += ControlInteractability;
            stateManager.GameStateSlotManager.OnSaved += ControlInteractability;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            stateManager.GameStateSlotManager.OnBeforeLoad -= ControlInteractability;
            stateManager.GameStateSlotManager.OnLoaded -= ControlInteractability;
            stateManager.GameStateSlotManager.OnBeforeSave -= ControlInteractability;
            stateManager.GameStateSlotManager.OnSaved -= ControlInteractability;
        }

        public void Initialize (ScriptNavigatorPanel navigator, Script script, ScriptPlayer player)
        {
            this.navigator = navigator;
            this.script = script;
            this.player = player;
            name = "PlayScript: " + script.Name;
            if (labelText) labelText.text = script.Name;
            isInitialized = true;
            UIComponent.interactable = true;
        }

        public void OnPointerEnter (PointerEventData eventData)
        {
            if (UIComponent.interactable)
                labelText.fontStyle = FontStyle.Bold;
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            labelText.fontStyle = FontStyle.Normal;
        }

        protected override void OnButtonClick ()
        {
            Debug.Assert(isInitialized);
            navigator.Hide();
            PlayScriptAsync();
        }

        private async void PlayScriptAsync ()
        {
            await stateManager.ResetStateAsync(() => player.PreloadAndPlayAsync(script));
        }

        private void ControlInteractability ()
        {
            UIComponent.interactable = !stateManager.GameStateSlotManager.IsLoading && !stateManager.GameStateSlotManager.IsSaving;
        }
    } 
}
