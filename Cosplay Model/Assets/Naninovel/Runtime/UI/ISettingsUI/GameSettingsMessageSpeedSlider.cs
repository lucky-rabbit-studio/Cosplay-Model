// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class GameSettingsMessageSpeedSlider : ScriptableSlider
    {
        private TextPrinterManager printerMngr;

        protected override void Awake ()
        {
            base.Awake();

            printerMngr = Engine.GetService<TextPrinterManager>();
        }

        protected override void Start ()
        {
            base.Start();

            UIComponent.value = printerMngr.PrintSpeed;
        }

        protected override void OnValueChanged (float value)
        {
            printerMngr.SetPrintSpeed(value);
        }
    }
}
