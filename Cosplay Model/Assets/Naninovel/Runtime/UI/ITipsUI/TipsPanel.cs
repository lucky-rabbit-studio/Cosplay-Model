// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TipsPanel : ScriptableUIBehaviour, ITipsUI
    {
        [System.Serializable]
        private class TipsSelectedState : SerializableMap<string, bool> { }

        public const string DefaultManagedTextCategory = "Tips";

        public int TipsCount { get; private set; }

        private const string separatorLiteral = "|";

        [Header("Tips Setup")]
        [Tooltip("All the unlockable item IDs with the specified prefix will be considered Tips items.")]
        [SerializeField] private string unlockableIdPrefix = "Tips";
        [Tooltip("The name of the managed text document (category) where all the tips data is stored.")]
        [SerializeField] private string managedTextCategory = DefaultManagedTextCategory;

        [Header("UI Setup")]
        [SerializeField] private RectTransform itemsContainer = default;
        [SerializeField] private TipsListItem itemPrefab = default;
        [SerializeField] private Text titleText = default;
        [SerializeField] private Text numberText = default;
        [SerializeField] private Text categoryText = default;
        [SerializeField] private Text descriptionText = default;

        private UnlockableManager unlockableManager;
        private TextManager textManager;
        private StateManager stateManager;
        private InputManager inputManager;
        private TipsSelectedState tipsSelectedState = new TipsSelectedState();
        private List<TipsListItem> listItems = new List<TipsListItem>();

        public Task InitializeAsync ()
        {
            tipsSelectedState = stateManager.GlobalState.DeserializeObject<TipsSelectedState>() ?? new TipsSelectedState();

            var records = textManager.GetAllRecords(managedTextCategory);
            foreach (var record in records)
            {
                var unlockableId = $"{unlockableIdPrefix}/{record.Key}";
                var title = record.Value.GetBefore(separatorLiteral) ?? record.Value;
                var selectedOnce = tipsSelectedState.TryGetValue(unlockableId, out var selected) && selected;
                var item = TipsListItem.Instantiate(itemPrefab, unlockableId, title, selectedOnce, HandleItemClicked);
                item.transform.SetParent(itemsContainer, false);
                listItems.Add(item);
            }

            foreach (var item in listItems)
                item.SetUnlocked(unlockableManager.ItemUnlocked(item.UnlockableId));

            TipsCount = listItems.Count;

            return Task.CompletedTask;
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(itemsContainer, itemPrefab, titleText, numberText, categoryText, descriptionText);

            unlockableManager = Engine.GetService<UnlockableManager>();
            textManager = Engine.GetService<TextManager>();
            stateManager = Engine.GetService<StateManager>();
            inputManager = Engine.GetService<InputManager>();

            titleText.text = string.Empty;
            numberText.text = string.Empty;
            categoryText.text = string.Empty;
            descriptionText.text = string.Empty;
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            OnVisibilityChanged += HandleVisibilityChanged;
            unlockableManager.OnItemUpdated += HandleUnlockableItemUpdated;
            inputManager.AddBlockingUI(this);
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            OnVisibilityChanged -= HandleVisibilityChanged;
            if (unlockableManager != null)
                unlockableManager.OnItemUpdated -= HandleUnlockableItemUpdated;
            inputManager?.RemoveBlockingUI(this);
        }

        private void HandleItemClicked (TipsListItem clickedItem)
        {
            if (!unlockableManager.ItemUnlocked(clickedItem.UnlockableId)) return;

            tipsSelectedState[clickedItem.UnlockableId] = true;
            foreach (var item in listItems)
                item.SetSelected(item.UnlockableId.EqualsFast(clickedItem.UnlockableId));
            var recordValue = textManager.GetRecordValue(clickedItem.UnlockableId.GetAfterFirst($"{unlockableIdPrefix}/"), managedTextCategory);
            titleText.text = recordValue.GetBefore(separatorLiteral)?.Trim() ?? recordValue;
            numberText.text = clickedItem.Number.ToString();
            categoryText.text = recordValue.GetBetween(separatorLiteral)?.Trim() ?? string.Empty;
            descriptionText.text = recordValue.GetAfter(separatorLiteral)?.Replace("\\n", "\n")?.Trim() ?? string.Empty;
        }

        private async void HandleVisibilityChanged (bool visible)
        {
            if (visible) return;

            stateManager.GlobalState.SerializeObject(tipsSelectedState);
            await stateManager.SaveGlobalStateAsync();
        }

        private void HandleUnlockableItemUpdated (UnlockableItemUpdatedArgs args)
        {
            if (!args.Id.StartsWithFast(unlockableIdPrefix)) return;
            listItems.FirstOrDefault(i => i.UnlockableId.EqualsFast(args.Id))?.SetUnlocked(args.Unlocked);
        }
    }
}
