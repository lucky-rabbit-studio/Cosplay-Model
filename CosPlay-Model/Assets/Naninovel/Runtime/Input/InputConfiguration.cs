// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class InputConfiguration : Configuration
    {
        [Tooltip("Bindings to process input for.")]
        public List<InputBinding> Bindings = new List<InputBinding> {
            new InputBinding { Name = InputManager.SubmitName, Keys = new List<KeyCode> { KeyCode.Mouse0 } },
            new InputBinding { Name = InputManager.CancelName, Keys = new List<KeyCode> { KeyCode.Escape }, AlwaysProcess = true },
            new InputBinding {
                Name = InputManager.ContinueName,
                Keys = new List<KeyCode> { KeyCode.Return, KeyCode.KeypadEnter, KeyCode.JoystickButton0 },
                Axes = new List<InputAxisTrigger> { new InputAxisTrigger { AxisName = "Mouse ScrollWheel", TriggerMode = InputAxisTriggerMode.Negative } }
            },
            new InputBinding { Name = InputManager.SkipName, Keys = new List<KeyCode> { KeyCode.LeftControl, KeyCode.RightControl, KeyCode.JoystickButton1 } },
            new InputBinding { Name = InputManager.AutoPlayName, Keys = new List<KeyCode> { KeyCode.A, KeyCode.JoystickButton2 } },
            new InputBinding { Name = InputManager.ToggleUIName, Keys = new List<KeyCode> { KeyCode.Space, KeyCode.JoystickButton3 } },
            new InputBinding {
                Name = InputManager.ShowBacklogName,
                Keys = new List<KeyCode> { KeyCode.B, KeyCode.JoystickButton4 },
                Axes = new List<InputAxisTrigger> { new InputAxisTrigger { AxisName = "Mouse ScrollWheel", TriggerMode = InputAxisTriggerMode.Positive } }
            }
        };
        [Tooltip("Limits frequency on the continue input when using touch input.")]
        public float TouchContinueCooldown = .1f;
        [Tooltip("Whether to spawn an event system and input module when initializing. Uncheck in case scenes will already have one.")]
        public bool SpawnEventSystem = true;
    }
}
