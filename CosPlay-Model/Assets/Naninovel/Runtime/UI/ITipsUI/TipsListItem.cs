// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    public class TipsListItem : MonoBehaviour
    {
        public string UnlockableId { get; private set; }
        public int Number => transform.GetSiblingIndex() + 1;

        [SerializeField] private Button button = default;
        [SerializeField] private Text label = default;
        [SerializeField] private GameObject selectedIndicator = default;

        private Action<TipsListItem> onClick;
        private string title;
        private bool selectedOnce;

        public static TipsListItem Instantiate (TipsListItem prototype, string unlockableId, string title, bool selectedOnce, Action<TipsListItem> onClick)
        {
            var item = Instantiate(prototype);

            item.onClick = onClick;
            item.UnlockableId = unlockableId;
            item.title = title;
            item.selectedOnce = selectedOnce;

            return item;
        }

        public void SetSelected (bool selected)
        {
            if (selected)
            {
                selectedOnce = true;
                label.fontStyle = FontStyle.Normal;
            }
            selectedIndicator.SetActive(selected);
        }

        public void SetUnlocked (bool unlocked)
        {
            label.text = $"{Number}. {(unlocked ? title : "???")}";
            label.fontStyle = !unlocked || selectedOnce ? FontStyle.Normal : FontStyle.Bold;
            button.interactable = unlocked;
        }

        private void Awake ()
        {
            this.AssertRequiredObjects(button, label, selectedIndicator);
            selectedIndicator.SetActive(false);
        }

        private void OnEnable ()
        {
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable ()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }

        private void HandleButtonClicked ()
        {
            onClick?.Invoke(this);
        }
    }
}
