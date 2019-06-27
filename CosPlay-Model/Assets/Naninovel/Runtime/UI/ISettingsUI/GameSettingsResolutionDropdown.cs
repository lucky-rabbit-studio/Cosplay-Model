// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsResolutionDropdown : ScriptableDropdown
    {
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
            InitializeOptions(Screen.resolutions.Select(r => r.ToString()).ToList());
            #endif
        }

        protected override void OnValueChanged (int value)
        {
            if (!allowApplySettings) return; // Prevent changing resolution when UI initializes.
            var resolution = Screen.resolutions[value];
            orthoCamera.SetResolution(new Vector2Int(resolution.width, resolution.height), orthoCamera.ScreenMode, resolution.refreshRate);
        }

        private void InitializeOptions (List<string> availableOptions)
        {
            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableOptions);
            UIComponent.value = orthoCamera.ResolutionIndex;
            UIComponent.RefreshShownValue();
            allowApplySettings = true;
        }
    }
}
