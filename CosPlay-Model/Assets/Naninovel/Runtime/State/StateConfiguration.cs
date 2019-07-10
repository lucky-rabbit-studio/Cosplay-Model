// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class StateConfiguration : Configuration
    {
        [Tooltip("The folder will be created in the game data folder.")]
        public string SaveFolderName = "Saves";
        [Tooltip("The name of the settings save file.")]
        public string DefaultSettingsSlotId = "Settings";
        [Tooltip("The name of the global save file.")]
        public string DefaultGlobalSlotId = "GlobalSave";
        [Tooltip("Mask used to name save slots.")]
        public string SaveSlotMask = "GameSave{0:000}";
        [Tooltip("Mask used to name quick save slots.")]
        public string QuickSaveSlotMask = "GameQuickSave{0:000}";
        [Tooltip("Maximum number of save slots."), Range(1, 999)]
        public int SaveSlotLimit = 99;
        [Tooltip("Maximum number of quick save slots."), Range(1, 999)]
        public int QuickSaveSlotLimit = 9;
        [Tooltip("Seconds to wait before starting the load operation.")]
        public float LoadStartDelay = 0.3f;
        [Tooltip("Whether to reset state of all the engine services and unload (dispose) resources upon loading a naninovel script. This is usually triggered when using `@goto` command to move playback to another script. It's recommended to leave this enabled to prevent memory leak issues. If you choose to disable this option, you can still reset the state and dispose resources manually at any time using `@resetState` command.")]
        public bool ResetStateOnLoad = true;
    }
}
