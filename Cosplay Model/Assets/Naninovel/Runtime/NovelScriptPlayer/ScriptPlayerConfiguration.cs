// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class ScriptPlayerConfiguration : Configuration
    {
        [Tooltip("Time scale to use when in skip (fast-forward) mode.")]
        public float SkipTimeScale = 10f;
        [Tooltip("Minimum seconds to wait before executing next action while in auto play mode.")]
        public float MinAutoPlayDelay = 3f;
        [Tooltip("Whether to calculate number of actions existing in all the available novel scripts on service initalization. If you don't use `TotalActionCount` property of the script player and `CalculateProgress` function in novel script expressions, disable to reduce engine initalization time.")]
        public bool UpdateActionCountOnInit = true;
    }
}
