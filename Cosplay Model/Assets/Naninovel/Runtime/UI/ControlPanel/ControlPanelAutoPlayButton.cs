// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class ControlPanelAutoPlayButton : ScriptableLabeledButton
    {
        [ManagedText("UIControlPanel")]
        public readonly static string LabelText = "AUTO";

        private NovelScriptPlayer player;

        protected override void Awake ()
        {
            base.Awake();

            player = Engine.GetService<NovelScriptPlayer>();
            UIComponent.Label.text = LabelText;
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
