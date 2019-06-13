// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class ControlPanelQuickLoadButton : ScriptableButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "Q.LOAD";

        private StateManager gameState;
        private NovelScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<StateManager>();
            player = Engine.GetService<NovelScriptPlayer>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void Start ()
        {
            base.Start();

            ControlInteractability();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            gameState.GameStateSlotManager.OnBeforeLoad += ControlInteractability;
            gameState.GameStateSlotManager.OnLoaded += ControlInteractability;
            gameState.GameStateSlotManager.OnBeforeSave += ControlInteractability;
            gameState.GameStateSlotManager.OnSaved += ControlInteractability;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            gameState.GameStateSlotManager.OnBeforeLoad -= ControlInteractability;
            gameState.GameStateSlotManager.OnLoaded -= ControlInteractability;
            gameState.GameStateSlotManager.OnBeforeSave -= ControlInteractability;
            gameState.GameStateSlotManager.OnSaved -= ControlInteractability;
        }

        protected override void OnButtonClick ()
        {
            UIComponent.interactable = false;
            QuickLoadAsync();
        }

        private async void QuickLoadAsync ()
        {
            await gameState.QuickLoadAsync();
            player.Play();
        }

        private void ControlInteractability ()
        {
            UIComponent.interactable = gameState.QuickLoadAvailable && !gameState.GameStateSlotManager.IsLoading && !gameState.GameStateSlotManager.IsSaving;
        }
    } 
}
