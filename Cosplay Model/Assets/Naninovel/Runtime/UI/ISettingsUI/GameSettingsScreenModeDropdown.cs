// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsScreenModeDropdown : ScriptableDropdown
    {
        [ManagedText("UIGameSettings")]
        public readonly static string ExclusiveFullScreen = "Full Screen";
        [ManagedText("UIGameSettings")]
        public readonly static string FullScreenWindow = "Full Screen Window";
        [ManagedText("UIGameSettings")]
        public readonly static string MaximizedWindow = "Maximized Window";
        [ManagedText("UIGameSettings")]
        public readonly static string Windowed = "Windowed";

        private CameraManager orthoCamera;
        private bool allowApplySettings;

        protected override void Awake ()
        {
            base.Awake();

            orthoCamera = Engine.GetService<CameraManager>();
        }

        protected override void Start ()
        {
            base.Start();

            #if !UNITY_STANDALONE && !UNITY_EDITOR
            transform.parent.gameObject.SetActive(false);
            #else
            var options = new List<string> { ExclusiveFullScreen, FullScreenWindow, MaximizedWindow, Windowed };
            InitializeOptions(options);
            #endif
        }

        protected override void OnValueChanged (int value)
        {
            if (!allowApplySettings) return; // Prevent changing resolution when UI initializes.
            orthoCamera.SetResolution(orthoCamera.Resolution, (FullScreenMode)value, orthoCamera.RefreshRate);
        }

        private void InitializeOptions (List<string> availableOptions)
        {
            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableOptions);
            UIComponent.value = (int)orthoCamera.ScreenMode;
            UIComponent.RefreshShownValue();
            allowApplySettings = true;
        }
    }
}
