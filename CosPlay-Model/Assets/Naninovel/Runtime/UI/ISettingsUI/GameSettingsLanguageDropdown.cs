// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsLanguageDropdown : ScriptableDropdown
    {
        private const string tempSaveSlotId = "TEMP_LOCALE_CHANGE";

        private LocalizationManager localizationManager;
        private Dictionary<int, string> optionToLocaleMap = new Dictionary<int, string>();

        protected override void Awake ()
        {
            base.Awake();

            localizationManager = Engine.GetService<LocalizationManager>();
            InitializeOptions(localizationManager.AvailableLocales);
        }

        protected override void OnValueChanged (int value)
        {
            var selectedLocale = optionToLocaleMap[value];
            HandleLocaleChangedAsync(selectedLocale);
        }

        private void InitializeOptions (List<string> availableLocales)
        {
            optionToLocaleMap.Clear();
            for (int i = 0; i < availableLocales.Count; i++)
                optionToLocaleMap.Add(i, availableLocales[i]);

            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableLocales.Select(l => LanguageTags.GetLanguageByTag(l)).ToList());
            UIComponent.value = availableLocales.IndexOf(localizationManager.SelectedLocale);
            UIComponent.RefreshShownValue();
        }

        private async void HandleLocaleChangedAsync (string locale)
        {
            var clickThroughPanel = Engine.GetService<UIManager>()?.GetUI<ClickThroughPanel>();
            clickThroughPanel?.Show(false, null);

            await localizationManager.SelectLocaleAsync(locale);

            var player = Engine.GetService<ScriptPlayer>();
            if (player.PlayedScript != null)
            {
                var wasPlaying = player.IsPlaying;
                var stateManager = Engine.GetService<StateManager>();
                await stateManager.SaveGameAsync(tempSaveSlotId);
                await stateManager.ResetStateAsync();
                await Engine.GetService<ScriptManager>().ReloadAllScriptsAsync();
                await stateManager.LoadGameAsync(tempSaveSlotId);
                stateManager.GameStateSlotManager.DeleteSaveSlot(tempSaveSlotId);
                if (wasPlaying) player.Play();
            }
            else await Engine.GetService<ScriptManager>().ReloadAllScriptsAsync();

            clickThroughPanel?.Hide();
        }
    }
}
