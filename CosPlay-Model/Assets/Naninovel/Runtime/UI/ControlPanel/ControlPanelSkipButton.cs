// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class ControlPanelSkipButton : ScriptableLabeledButton
    {
        private ScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<ScriptPlayer>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();
            player.OnSkip += HandleSkipModeChange;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            player.OnSkip -= HandleSkipModeChange;
        }

        protected override void OnButtonClick ()
        {
            if (player.IsSkipActive) player.DisableSkip();
            else player.EnableSkip();
        }

        private void HandleSkipModeChange (bool enabled)
        {
            UIComponent.LabelColorMultiplier = enabled ? Color.red : Color.white;
        }
    } 
}
