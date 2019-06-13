// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsLanguageDropdown : ScriptableDropdown
    {
        private LocalizationManager localizationMngr;
        private Dictionary<int, string> optionToLocaleMap = new Dictionary<int, string>();

        protected override void Awake ()
        {
            base.Awake();

            localizationMngr = Engine.GetService<LocalizationManager>();
            InitializeOptions(localizationMngr.AvailableLocales);
        }

        protected override void OnValueChanged (int value)
        {
            var selectedLocale = optionToLocaleMap[value];
            localizationMngr.SelectLocaleAsync(selectedLocale).WrapAsync();
        }

        private void InitializeOptions (List<string> availableLocales)
        {
            optionToLocaleMap.Clear();
            for (int i = 0; i < availableLocales.Count; i++)
                optionToLocaleMap.Add(i, availableLocales[i]);

            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableLocales.Select(l => LanguageTags.GetLanguageByTag(l)).ToList());
            UIComponent.value = availableLocales.IndexOf(localizationMngr.SelectedLocale);
            UIComponent.RefreshShownValue();
        }
    }
}
