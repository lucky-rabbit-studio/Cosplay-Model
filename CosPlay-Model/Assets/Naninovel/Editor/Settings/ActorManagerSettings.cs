// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommon;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Naninovel
{
    /// <summary>
    /// Derive from this class to create custom editors for <see cref="Configuration"/> assets of <see cref="ActorManager"/> services.
    /// </summary>
    /// <typeparam name="TConfig">Type of the configuration asset this editor is built for.</typeparam>
    /// <typeparam name="TActor">Type of the actor this editor is built for.</typeparam>
    /// <typeparam name="TMeta">Type of the actor meta the actor manager uses.</typeparam>
    public abstract class ActorManagerSettings<TConfig, TActor, TMeta> : ResourcefulSettings<TConfig> 
        where TConfig : Configuration 
        where TActor : IActor
        where TMeta : ActorMetadata
    {
        protected static readonly string[] ActorImplementations = FindActorImplementations();

        protected SerializedProperty MetadataMapProperty { get; private set; }
        protected string EditedActorId => IsEditingMetadata ? MetadataMapEditor.SelectedActorId : null;
        protected TMeta EditedMetadata => IsEditingMetadata ? MetadataMapEditor.EditedMetadataProperty.GetGenericValue<TMeta>() : DefaultMetadata;
        protected TMeta DefaultMetadata { get; private set; }
        protected bool IsEditingMetadata => MetadataMapEditor.EditedMetadataProperty != null;
        protected GUIContent FromMetaButtonLabel { get; }
        protected MetadataMapEditor MetadataMapEditor { get; private set; }
        protected virtual string MetadataMapPropertyName => "Metadata";
        protected virtual string DefaultMetadataPropertyName => "DefaultMetadata";
        protected virtual Dictionary<string, Action<SerializedProperty>> OverrideMetaDrawers { get; }
        protected virtual bool AllowMultipleResources => false;
        protected virtual HashSet<string> LockedActorIds => null;
        protected override string ResourcesPathPrefix => AllowMultipleResources ? $"{EditedMetadata.LoaderConfiguration.PathPrefix}/{EditedActorId}" : EditedMetadata.LoaderConfiguration.PathPrefix;
        protected override string ResourcesCategoryId => MetadataToResourcesCategoryId(EditedMetadata);
        protected override string ResourceName => AllowMultipleResources ? null : EditedActorId;
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => new Dictionary<string, Action<SerializedProperty>> {
            [MetadataMapPropertyName] = null,
            [DefaultMetadataPropertyName] = property => {
                var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
                if (!property.isExpanded) return;
                EditorGUI.indentLevel++;
                DrawDefaultMetaEditor(property);
                EditorGUI.indentLevel--;
            }
        };

        private Dictionary<string, Action<SerializedProperty>> overrideDrawers;
        private HashSet<string> lockedActorIds;

        public ActorManagerSettings ()
        {
            overrideDrawers = OverrideMetaDrawers;
            lockedActorIds = LockedActorIds;
            FromMetaButtonLabel = new GUIContent ($"< Back To {EditorTitle} List");
        }

        public override void OnActivate (string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            MetadataMapProperty = SerializedObject.FindProperty(MetadataMapPropertyName);
            MetadataMapEditor = new MetadataMapEditor(SerializedObject, MetadataMapProperty, typeof(TMeta), EditorTitle, lockedActorIds);
            DefaultMetadata = SerializedObject.FindProperty(DefaultMetadataPropertyName).GetGenericValue<TMeta>();

            MetadataMapEditor.OnElementModified += HandleMetadataElementModified;
        }

        public override void OnDeactivate ()
        {
            base.OnDeactivate();

            if (MetadataMapEditor != null)
                MetadataMapEditor.OnElementModified -= HandleMetadataElementModified;
        }

        protected virtual string MetadataToResourcesCategoryId (ActorMetadata metadata) => $"{metadata.LoaderConfiguration.PathPrefix}/{metadata.Guid}";

        protected override void DrawConfigurationEditor ()
        {
            if (ShowResourcesEditor)
            {
                if (IsEditingMetadata)
                {
                    if (GUILayout.Button(FromMetaButtonLabel, GUIStyles.NavigationButton))
                        MetadataMapEditor.ResetEditedMetadata();
                    else
                    {
                        EditorGUILayout.Space();
                        DrawMetaEditor(MetadataMapEditor.EditedMetadataProperty);
                    }
                }
                else
                {
                    if (GUILayout.Button(FromResourcesButtonContent, GUIStyles.NavigationButton))
                        ShowResourcesEditor = false;
                    else
                    {
                        EditorGUILayout.Space();
                        MetadataMapEditor.DrawGUILayout();
                    }
                }
            }
            else
            {
                DrawDefaultEditor();

                EditorGUILayout.Space();
                if (GUILayout.Button(ToResourcesButtonContent, GUIStyles.NavigationButton))
                    ShowResourcesEditor = true;
            }
        }

        protected virtual void DrawMetaEditor (SerializedProperty metaProperty)
        {
            var actorTitle = MetadataMapEditor.SelectedActorId.InsertCamel();

            EditorGUILayout.LabelField($"{actorTitle} Metadata", EditorStyles.boldLabel);
            DrawDefaultMetaEditor(metaProperty);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(actorTitle + (AllowMultipleResources ? " Resources" : " Resource"), EditorStyles.boldLabel);
            ResourcesEditor.DrawGUILayout(ResourcesCategoryId, ResourcesPathPrefix, ResourceName, ResourcesTypeConstraint, ResourcesSelectionTooltip);

            // Return to meta list when pressing return key and no text fields are edited.
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Backspace && !EditorGUIUtility.editingTextField)
            {
                MetadataMapEditor.ResetEditedMetadata();
                Event.current.Use();
            }
        }

        protected void DrawDefaultMetaEditor (SerializedProperty metaProperty)
        {
            var property = metaProperty.Copy();
            var endProperty = property.GetEndProperty();

            property.NextVisible(true);
            do
            {
                if (SerializedProperty.EqualContents(property, endProperty))
                    break;

                if (overrideDrawers != null)
                {
                    var localPath = property.propertyPath.Replace(metaProperty.propertyPath + ".", string.Empty);
                    if (overrideDrawers.ContainsKey(localPath))
                    {
                        overrideDrawers[localPath]?.Invoke(property);
                        continue;
                    }
                }

                if (property.propertyPath.EndsWithFast("Implementation"))
                {
                    var label = EditorGUI.BeginProperty(Rect.zero, null, property);
                    var curIndex = ArrayUtility.IndexOf(ActorImplementations, property.stringValue ?? string.Empty);
                    var newIndex = EditorGUILayout.Popup(label, curIndex, ActorImplementations);
                    property.stringValue = ActorImplementations.IsIndexValid(newIndex) ? ActorImplementations[newIndex] : string.Empty;
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }
            while (property.NextVisible(false));
        }

        protected virtual void HandleMetadataElementModified (MetadataMapEditor.ElementModifiedArgs args)
        {
            // Remove resources category associated with the removed actor.
            if (args.ModificationType == MetadataMapEditor.ElementModificationType.Remove)
            {
                var categoryId = MetadataToResourcesCategoryId(args.Metadata);
                ResourcesEditor.RemoveCategory(categoryId);
            }
        }

        private static string[] FindActorImplementations ()
        {
            return ReflectionUtils.ExportedDomainTypes
                .Where(t => !t.IsAbstract && t.GetInterfaces().Contains(typeof(TActor)))
                .Select(t => t.FullName).ToArray();
        }
    }
}
