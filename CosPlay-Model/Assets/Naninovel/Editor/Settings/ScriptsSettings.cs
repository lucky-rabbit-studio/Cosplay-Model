// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class ScriptsSettings : ResourcefulSettings<ScriptsConfiguration>
    {
        protected override string HelpUri => "guide/naninovel-scripts.html";

        protected override Type ResourcesTypeConstraint => typeof(ScriptAsset);
        protected override string ResourcesCategoryId => Configuration.Loader.PathPrefix;
        protected override string ResourcesSelectionTooltip => "Use `@goto %name%` in naninovel scripts to load and start playing selected naninovel script.";
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            [nameof(ScriptsConfiguration.GlobalDefinesScript)] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix, "None (disabled)"),
            [nameof(ScriptsConfiguration.InitializationScript)] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix, "None (disabled)"),
            [nameof(ScriptsConfiguration.TitleScript)] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix, "None (disabled)"),
            [nameof(ScriptsConfiguration.StartGameScript)] = property => EditorResources.DrawPathPopup(property, ResourcesCategoryId, ResourcesPathPrefix),
            [nameof(ScriptsConfiguration.ExternalLoader)] = property => { if (Configuration.EnableCommunityModding) EditorGUILayout.PropertyField(property); },
            [nameof(ScriptsConfiguration.ShowNavigatorOnInit)] = property => { if (Configuration.EnableNavigator) EditorGUILayout.PropertyField(property); },
            [nameof(ScriptsConfiguration.NavigatorSortOrder)] = property => { if (Configuration.EnableNavigator) EditorGUILayout.PropertyField(property); },
        };

        [MenuItem("Naninovel/Resources/Scripts")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
