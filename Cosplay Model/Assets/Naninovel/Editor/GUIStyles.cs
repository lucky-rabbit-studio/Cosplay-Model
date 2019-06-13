// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public static class GUIStyles
    {
        public static readonly GUIStyle NavigationButton;
        public static readonly GUIStyle IconButton;

        static GUIStyles ()
        {
            NavigationButton = new GUIStyle("AC Button");
            NavigationButton.stretchWidth = true;
            NavigationButton.fixedWidth = 0;

            IconButton = GetStyle("IconButton"); 
        }

        private static GUIStyle GetStyle (string styleName)
        {
            var style = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (style is null) Debug.LogError($"Missing built-in guistyle `{styleName}`.");
            return style;
        }
    }
}
