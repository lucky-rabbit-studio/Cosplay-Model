// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SaveLoadMenu : ScriptableUIBehaviour, ISaveLoadUI
    {
        public SaveLoadUIPresentationMode PresentationMode { get => presentationMode; set => SetPresentationMode(value); }

        [Header("Tabs")]
        [SerializeField] private Toggle quickLoadToggle = null;
        [SerializeField] private Toggle saveToggle = null;
        [SerializeField] private Toggle loadToggle = null;

        [Header("Grids")]
        [SerializeField] private GameStateSlotsGrid quickLoadGrid = null;
        [SerializeField] private GameStateSlotsGrid saveGrid = null;
        [SerializeField] private GameStateSlotsGrid loadGrid = null;

        private StateManager stateManager;
        private InputManager inputManager;
        private IConfirmationUI confirmationUI;
        private SaveLoadUIPresentationMode presentationMode;
        private GameStateSlotManager SlotManager => stateManager?.GameStateSlotManager;

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(quickLoadToggle, saveToggle, loadToggle, quickLoadGrid, saveGrid, loadGrid);
            stateManager = Engine.GetService<StateManager>();
            inputManager = Engine.GetService<InputManager>();
            confirmationUI = Engine.GetService<UIManager>().GetUI<IConfirmationUI>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.OnGameSaveFinished += HandleGameSaveFinished;
            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            stateManager.OnGameSaveFinished -= HandleGameSaveFinished;
            inputManager.RemoveBlockingUI(this);
        }

        public async Task InitializeAsync ()
        {
            var saveSlots = await SlotManager.LoadAllSaveSlotsAsync();
            foreach (var slot in saveSlots)
            {
                saveGrid.AddSlot(new GameStateSlot.Constructor(saveGrid.SlotPrototype, slot.Key, slot.Value, HandleSaveSlotClicked, HandleDeleteSlotClicked).ConstructedSlot);
                loadGrid.AddSlot(new GameStateSlot.Constructor(loadGrid.SlotPrototype, slot.Key, slot.Value, HandleLoadSlotClicked, HandleDeleteSlotClicked).ConstructedSlot);
            }

            var quickSaveSlots = await SlotManager.LoadAllQuickSaveSlotsAsync();
            foreach (var slot in quickSaveSlots)
                quickLoadGrid.AddSlot(new GameStateSlot.Constructor(saveGrid.SlotPrototype, slot.Key, slot.Value, HandleLoadSlotClicked, HandleDeleteQuickLoadSlotClicked).ConstructedSlot);
        }

        public SaveLoadUIPresentationMode GetLastLoadMode ()
        {
            var qLoadTime = quickLoadGrid.LastSaveDateTime;
            var loadTime = loadGrid.LastSaveDateTime;

            if (!qLoadTime.HasValue) return SaveLoadUIPresentationMode.Load;
            if (!loadTime.HasValue) return SaveLoadUIPresentationMode.QuickLoad;

            return quickLoadGrid.LastSaveDateTime > loadGrid.LastSaveDateTime ? 
                SaveLoadUIPresentationMode.QuickLoad : SaveLoadUIPresentationMode.Load;
        }

        private void SetPresentationMode (SaveLoadUIPresentationMode value)
        {
            presentationMode = value;
            switch (value)
            {
                case SaveLoadUIPresentationMode.QuickLoad:
                    loadToggle.gameObject.SetActive(true);
                    quickLoadToggle.gameObject.SetActive(true);
                    quickLoadToggle.isOn = true;
                    saveToggle.gameObject.SetActive(false);
                    break;
                case SaveLoadUIPresentationMode.Load:
                    loadToggle.gameObject.SetActive(true);
                    quickLoadToggle.gameObject.SetActive(true);
                    loadToggle.isOn = true;
                    saveToggle.gameObject.SetActive(false);
                    break;
                case SaveLoadUIPresentationMode.Save:
                    saveToggle.gameObject.SetActive(true);
                    saveToggle.isOn = true;
                    loadToggle.gameObject.SetActive(false);
                    quickLoadToggle.gameObject.SetActive(false);
                    break;
            }
        }

        private async void HandleLoadSlotClicked (string slotId)
        {
            if (!SlotManager.SaveSlotExists(slotId)) return;
            Hide();
            await stateManager.LoadGameAsync(slotId);
            Engine.GetService<ScriptPlayer>().Play();
        }

        private async void HandleSaveSlotClicked (string slotId)
        {
            SetIsInteractable(false);

            if (SlotManager.SaveSlotExists(slotId))
            {
                var confirmed = await confirmationUI.ConfirmAsync(SaveLoadMenuManagedText.OverwriteSaveSlotMessage);
                if (!confirmed)
                {
                    SetIsInteractable(true);
                    return;
                }
            }

            var state = await stateManager.SaveGameAsync(slotId);

            saveGrid.GetSlot(slotId).SetState(state);
            loadGrid.GetSlot(slotId).SetState(state);

            SetIsInteractable(true);
        }

        private async void HandleDeleteSlotClicked (string slotId)
        {
            if (!SlotManager.SaveSlotExists(slotId)) return;

            if (!await confirmationUI.ConfirmAsync(SaveLoadMenuManagedText.DeleteSaveSlotMessage)) return;

            SlotManager.DeleteSaveSlot(slotId);
            saveGrid.GetSlot(slotId).SetEmptyState();
            loadGrid.GetSlot(slotId).SetEmptyState();
        }

        private async void HandleDeleteQuickLoadSlotClicked (string slotId)
        {
            if (!SlotManager.SaveSlotExists(slotId)) return;

            if (!await confirmationUI.ConfirmAsync(SaveLoadMenuManagedText.DeleteSaveSlotMessage)) return;

            SlotManager.DeleteSaveSlot(slotId);
            quickLoadGrid.GetSlot(slotId).SetEmptyState();
        }

        private void HandleGameSaveFinished (GameSaveLoadArgs args)
        {
            if (!args.Quick) return;

            // Shifting quick save slots by one to free the first slot.
            for (int i = SlotManager.QuickSaveSlotLimit - 1; i > 0; i--)
            {
                var currSlotId = SlotManager.IndexToQuickSaveSlotId(i);
                var prevSlotId = SlotManager.IndexToQuickSaveSlotId(i + 1);
                var currSlot = quickLoadGrid.GetSlot(currSlotId);
                var prevSlot = quickLoadGrid.GetSlot(prevSlotId);
                prevSlot.SetState(currSlot.State);
            }
            // Setting the new quick save to the first slot.
            var firstSlotId = SlotManager.IndexToQuickSaveSlotId(1);
            quickLoadGrid.GetSlot(firstSlotId).SetState(args.StateMap);
        }
    }
}
