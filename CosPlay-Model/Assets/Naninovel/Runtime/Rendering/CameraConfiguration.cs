// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class CameraConfiguration : Configuration
    {
        [Tooltip("The reference resolution is used to evaluate proper rendering dimensions, so that sprite assets (like backgrounds and characters) are correctly positioned on scene. As a rule of thumb, set this equal to the resolution of the background textures you make for the game.")]
        public Vector2Int ReferenceResolution = new Vector2Int(1920, 1080);
        [Tooltip("Initial world position of the camera.")]
        public Vector3 InitialPosition = new Vector3(0, 0, -10);
        [Tooltip("A prefab with a camera component to use for rendering. Will use a default one when not specified. In case you wish to set some camera properties (background color, FOV, HDR, etc) or add post-processing scripts, create a prefab with the desired camera setup and assign the prefab to this field.")]
        public Camera CustomCameraPrefab = null;
        [Tooltip("Whether to render the UI in a separate camera. This will allow to use individual configuration for the main and UI cameras and prevent post-processing (image) effects from affecting the UI at the cost of a slight rendering overhead.")]
        public bool UseUICamera = true;
        [Tooltip("A prefab with a camera component to use for UI rendering. Will use a default one when not specified. Has no effect when `UseUICamera` is disabled")]
        public Camera CustomUICameraPrefab = null;
        [Tooltip("Eeasing function to use by default for all the camera modifications (changing zoom, position, rotation, etc).")]
        public EasingType DefaultEasing = EasingType.Linear;

        [Header("Thumbnails")]
        [Tooltip("The resolution in which thumbnails to preview game save slots will be captured.")]
        public Vector2Int ThumbnailResolution = new Vector2Int(240, 140);
        [Tooltip("Whether to ignore UI layer when capturing thumbnails.")]
        public bool HideUIInThumbnails = false;
    }
}
