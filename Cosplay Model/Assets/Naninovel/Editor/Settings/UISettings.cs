// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Naninovel
{
    public class UISettings : ConfigurationSettings<UIConfiguration>
    {
        protected override string HelpUri => "guide/ui-customization.html";

        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            ["ObjectsLayer"] = (property) => {
                var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                property.intValue = EditorGUILayout.LayerField(label, property.intValue);
            },
            ["CustomUI"] = null
        };

        private ReorderableList reorderableList;

        protected override void DrawConfigurationEditor ()
        {
            DrawDefaultEditor();

            EditorGUILayout.Space();

            // Always check list's serialized object parity with the inspected object.
            if (reorderableList is null || reorderableList.serializedProperty.serializedObject != SerializedObject)
                InitilizeList();

            reorderableList.DoLayoutList();
        }

        private void InitilizeList ()
        {
            reorderableList = new ReorderableList(SerializedObject, SerializedObject.FindProperty("CustomUI"), true, true, true, true);
            reorderableList.drawHeaderCallback = DrawListHeader;
            reorderableList.drawElementCallback = DrawListElement;
        }

        private void DrawListHeader (Rect rect)
        {
            var label = EditorGUI.BeginProperty(Rect.zero, null, reorderableList.serializedProperty);
            var propertyRect = new Rect(5 + rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(propertyRect, label);
        }

        private void DrawListElement (Rect rect, int index, bool isActive, bool isFocused)
        {
            var elementProperty = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            var propertyRect = new Rect(rect.x, rect.y + EditorGUIUtility.standardVerticalSpacing, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.ObjectField(propertyRect, elementProperty, typeof(GameObject), GUIContent.none);
        }
    }
}
