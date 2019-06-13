// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class ScriptsSettings : ResourcefulSettings<ScriptsConfiguration>
    {
        protected override string HelpUri => "guide/novel-scripts.html";

        protected override Type ResourcesTypeConstraint => typeof(TextAsset);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override string ResourcesSelectionTooltip => "Use `@goto %name%` in novel scripts to load and start playing selected novel script.";
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            ["InitializationScript"] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix, "None (disabled)"),
            ["TitleScript"] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix, "None (disabled)"),
            ["StartGameScript"] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix),
            ["ExternalLoader"] = property => { if (Configuration.EnableCommunityModding) EditorGUILayout.PropertyField(property); },
            ["ShowNavigatorOnInit"] = property => { if (Configuration.EnableNavigator) EditorGUILayout.PropertyField(property); },
            ["NavigatorSortOrder"] = property => { if (Configuration.EnableNavigator) EditorGUILayout.PropertyField(property); },
        };

        [MenuItem("Naninovel/Resources/Novel Scripts")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
