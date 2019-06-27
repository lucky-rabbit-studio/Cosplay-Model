// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class LocalizationSettings : ConfigurationSettings<LocalizationConfiguration>
    {
        protected override string HelpUri => "guide/localization.html";
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            [nameof(LocalizationConfiguration.DefaultLocale)] = property => LocalesPopupDrawer.Draw(property)
        };

        protected override void DrawConfigurationEditor ()
        {
            DrawDefaultEditor();

            EditorGUILayout.Space();

            if (GUILayout.Button("Open Localization Utility", GUIStyles.NavigationButton))
                LocalizationWindow.OpenWindow();
        }
    }
}
