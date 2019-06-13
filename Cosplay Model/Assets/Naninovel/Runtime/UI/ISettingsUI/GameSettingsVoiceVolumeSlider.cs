// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsVoiceVolumeSlider : ScriptableSlider
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

            if (!audioMngr.VoiceGroupAvailable)
                transform.parent.gameObject.SetActive(false);
            else UIComponent.value = audioMngr.SfxVolume;
        }

        protected override void OnValueChanged (float value)
        {
            audioMngr.VoiceVolume = value;
        }
    }
}
