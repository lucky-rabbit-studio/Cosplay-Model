// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class ControlPanelAutoPlayButton : ScriptableLabeledButton
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
            player.OnAutoPlay += HandleAutoModeChange;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            player.OnAutoPlay -= HandleAutoModeChange;
        }

        protected override void OnButtonClick () => player.ToggleAutoPlay();

        private void HandleAutoModeChange (bool enabled)
        {
            UIComponent.LabelColorMultiplier = enabled ? Color.red : Color.white;
        }
    } 
}
