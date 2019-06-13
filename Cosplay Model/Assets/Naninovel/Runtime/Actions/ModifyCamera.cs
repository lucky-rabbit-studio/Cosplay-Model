// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Modifies the main camera, changing offset, zoom level and rotation over time.
    /// Check [this video](https://youtu.be/zy28jaMss8w) for a quick demonstration of the action effect.
    /// </summary>
    /// <example>
    /// ; Offset over X-axis (pan) the camera by -3 units and offset over Y-axis by 1.5 units
    /// @camera offset:-3,1.5
    /// 
    /// ; Zoom-in the camera by 50%.
    /// @camera zoom:0.5
    /// 
    /// ; Rotate the camera by 10 degrees clock-wise
    /// @camera rotation:10
    /// 
    /// ; All the above simultaneously animated over 5 seconds
    /// @camera offset:-3,1.5 zoom:0.5 rotation:10 time:5
    /// 
    /// ; Instantly reset camera to the default state
    /// @camera offset:0,0 zoom:0 rotation:0 time:0
    /// 
    /// ; Toggle `FancyCameraFilter` and `Bloom` components attached to the camera
    /// @camera toggle:FancyCameraFilter,Bloom
    /// </example>
    [ActionAlias("camera")]
    public class ModifyCamera : NovelAction
    {
        private struct UndoData { public bool Executed; public Vector2 Offset; public float Rotation, Zoom; public string[] ToggleTypeNames; }

        /// <summary>
        /// Local camera position offset in units by X and Y axis.
        /// </summary>
        [ActionParameter(optional: true)]
        public float?[] Offset { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Local camera rotation by Z-axis in angle degrees (0.0 to 360.0 or -180.0 to 180.0).
        /// </summary>
        [ActionParameter(optional: true)]
        public float Rotation { get => GetDynamicParameter(0f); set => SetDynamicParameter(value); }
        /// <summary>
        /// Relatize camera zoom (orthographic size scale), in 0.0 to 1.0 range.
        /// </summary>
        [ActionParameter(optional: true)]
        public float Zoom { get => GetDynamicParameter(0f); set => SetDynamicParameter(value); }
        /// <summary>
        /// Names of the components to toggle (enable if disabled and vice-versa). The components should be attached to the same gameobject as the camera.
        /// This can be used to toggle [custom post-processing effects](/guide/special-effects.md#camera-effects).
        /// </summary>
        [ActionParameter("toggle", true)]
        public string[] ToggleTypeNames { get => GetDynamicParameter<string[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Name of the easing function to use for the modification.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the camera configuration settings.
        /// </summary>
        [ActionParameter("easing", true)]
        public string EasingTypeName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var camera = Engine.GetService<CameraManager>();

            undoData.Executed = true;
            undoData.Offset = camera.Offset;
            undoData.Rotation = camera.Rotation;
            undoData.Zoom = camera.Zoom;
            undoData.ToggleTypeNames = ToggleTypeNames;

            var easingType = camera.DefaultEasintType;
            if (!string.IsNullOrEmpty(EasingTypeName) && !Enum.TryParse(EasingTypeName, true, out easingType))
                Debug.LogWarning($"Failed to parse `{EasingTypeName}` easing.");

            if (undoData.ToggleTypeNames != null)
                foreach (var name in ToggleTypeNames)
                    ToggleComponent(name, camera.Camera.gameObject);

            await Task.WhenAll(
                    camera.ChangeOffsetAsync(ArrayUtils.ToVector2(Offset, Vector2.zero), Duration, easingType),
                    camera.ChangeRotationAsync(Rotation, Duration, easingType),
                    camera.ChangeZoomAsync(Zoom, Duration, easingType)
                );
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var camera = Engine.GetService<CameraManager>();

            camera.Offset = undoData.Offset;
            camera.Rotation = undoData.Rotation;
            camera.Zoom = undoData.Zoom;

            if (undoData.ToggleTypeNames != null)
                foreach (var name in ToggleTypeNames)
                    ToggleComponent(name, camera.Camera.gameObject);

            undoData = default;
            return Task.CompletedTask;
        }

        private void ToggleComponent (string componentName, GameObject obj)
        {
            var cmp = obj.GetComponent(componentName) as MonoBehaviour;
            if (!cmp)
            {
                Debug.LogWarning($"Novel script `{ScriptName}` at #{LineNumber}: Failed to toggle `{componentName}` camera component; the component is not found on the camera's gameobject.");
                return;
            }
            cmp.enabled = !cmp.enabled;
        }
    }
}
