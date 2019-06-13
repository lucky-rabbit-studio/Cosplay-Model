// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsSkipModeDropdown : ScriptableDropdown
    {
        [ManagedText("UIGameSettings")]
        public readonly static string ReadOnly = "Read Only";
        [ManagedText("UIGameSettings")]
        public readonly static string Everything = "Everything";

        private NovelScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<NovelScriptPlayer>();
        }

        protected override void Start ()
        {
            base.Start();

            var options = new List<string> { ReadOnly, Everything };
            InitializeOptions(options);
        }

        protected override void OnValueChanged (int value)
        {
            player.SkipMode = (PlayerSkipMode)value;
        }

        private void InitializeOptions (List<string> availableOptions)
        {
            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableOptions);
            UIComponent.value = (int)player.SkipMode;
            UIComponent.RefreshShownValue();
        }
    }
}
