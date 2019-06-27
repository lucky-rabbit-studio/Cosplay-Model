// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;

namespace Naninovel
{
    public class ResourceProviderSettings : ConfigurationSettings<ResourceProviderConfiguration>
    {
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            [nameof(ResourceProviderConfiguration.DynamicPolicySteps)] = property => { if (Configuration.ResourcePolicy == ResourcePolicy.Dynamic) EditorGUILayout.PropertyField(property); },
            [nameof(ResourceProviderConfiguration.OptimizeLoadingPriority)] = property => { if (Configuration.ResourcePolicy == ResourcePolicy.Dynamic) EditorGUILayout.PropertyField(property); },
        };
    }
}
