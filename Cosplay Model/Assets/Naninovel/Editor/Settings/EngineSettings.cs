// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class EngineSettings : ConfigurationSettings<EngineConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            ["GeneratedDataPath"] = property => {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(property);
                if (EditorGUI.EndChangeCheck())
                    GeneratedDataPath = property.stringValue;
            },
            ["CustomInitializationUI"] = property => { if (Configuration.ShowInitializationUI) EditorGUILayout.PropertyField(property); },
            ["ObjectsLayer"] = property => {
                if (!Configuration.OverrideObjectsLayer) return;
                var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                property.intValue = EditorGUILayout.LayerField(label, property.intValue);
            },
            ["ToggleConsoleKey"] = property => { if (Configuration.EnableDevelopmentConsole) EditorGUILayout.PropertyField(property); }
        };
    }
}
