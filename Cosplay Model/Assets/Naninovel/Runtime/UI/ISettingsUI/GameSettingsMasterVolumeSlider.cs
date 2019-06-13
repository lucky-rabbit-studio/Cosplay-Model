// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsMasterVolumeSlider : ScriptableSlider
    {
        private AudioManager audioMngr;

        protected override void Awake ()
        {
            base.Awake();

            audioMngr = Engine.GetService<AudioManager>();
        }

        protected override void Start ()
        {
            base.Start();

            UIComponent.value = audioMngr.MasterVolume;
        }

        protected override void OnValueChanged (float value)
        {
            audioMngr.MasterVolume = value;
        }
    }
}
