// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.UI;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class UIConfiguration : Configuration
    {
        [Tooltip("The layer to assign for the UI elements instatiated by the engine. Used to cull the UI when using `toogle UI` feature.")]
        public int ObjectsLayer = 5;
        [Tooltip("The canvas render mode to apply for all the managed UI elements.")]
        public RenderMode RenderMode = RenderMode.ScreenSpaceCamera;
        [Tooltip("The sorting offset to apply for all the managed UI elements.")]
        public int SortingOffset = 1;
        [Tooltip("The list of custom UI prefabs to spawn on the engine initialization. Each prefab should have a `" + nameof(IManagedUI) + "`-derived component attached to the root object.")]
        public List<GameObject> CustomUI = new List<GameObject>();
    }
}
