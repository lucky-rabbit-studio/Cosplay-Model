// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsSkipModeDropdown : ScriptableDropdown
    {
        [ManagedText("UIGameSettings")]
        public static string ReadOnly = "Read Only";
        [ManagedText("UIGameSettings")]
        public static string Everything = "Everything";

        private ScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<ScriptPlayer>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            InitializeOptions();
        }

        protected override void OnValueChanged (int value)
        {
            player.SkipMode = (PlayerSkipMode)value;
        }

        private void InitializeOptions ()
        {
            var options = new List<string> { ReadOnly, Everything };
            UIComponent.ClearOptions();
            UIComponent.AddOptions(options);
            UIComponent.value = (int)player.SkipMode;
            UIComponent.RefreshShownValue();
        }
    }
}
