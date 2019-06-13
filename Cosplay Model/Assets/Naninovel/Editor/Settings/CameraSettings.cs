// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class CameraSettings : ConfigurationSettings<CameraConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            ["CustomUICameraPrefab"] = property => { if (Configuration.UseUICamera) EditorGUILayout.PropertyField(property); },
        };
    }
}
