// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityCommon;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class CharactersSettings : ActorManagerSettings<CharactersConfiguration, ICharacterActor, CharacterMetadata>
    {
        private static readonly GUIContent AvatarsEditorContent = new GUIContent("Avatar Resources",
            "Use 'CharacterId/Appearance' name to map avatar texture to a character appearance. Use 'CharacterId/Default' to map a default avatar to the character.");

        protected override string HelpUri => "guide/characters.html";
        protected override Type ResourcesTypeConstraint => GetTypeConstraint();
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override bool AllowMultipleResources => EditedMetadata?.Implementation == typeof(SpriteCharacter).FullName;
        protected override Dictionary<string, Action<SerializedProperty>> OverrideConfigurationDrawers => GetOverrideConfigurationDrawers();
        protected override Dictionary<string, Action<SerializedProperty>> OverrideMetaDrawers => new Dictionary<string, Action<SerializedProperty>> {
            ["DisplayName"] = property => {
                EditorGUILayout.PropertyField(property);
                if (!string.IsNullOrEmpty(property.stringValue))
                    EditorGUILayout.HelpBox("You can use managed text documents to localize (translate) the display names. See documentation on `Display Names` and `Managed Text` for more info.", MessageType.Info);
            },
            ["NameColor"] = property => { if (EditedMetadata.UseCharacterColor) EditorGUILayout.PropertyField(property); },
            ["MessageColor"] = property => { if (EditedMetadata.UseCharacterColor) EditorGUILayout.PropertyField(property); },
            ["SpeakingTint"] = property => { if (EditedMetadata.HighlightWhenSpeaking) EditorGUILayout.PropertyField(property); },
            ["NotSpeakingTint"] = property => { if (EditedMetadata.HighlightWhenSpeaking) EditorGUILayout.PropertyField(property); },
        };

        private bool avatarsEditorExpanded;

        private Type GetTypeConstraint ()
        {
            switch (EditedMetadata?.Implementation?.GetAfter("."))
            {
                case nameof(SpriteCharacter): return typeof(UnityEngine.Texture2D);
                case nameof(AnimatorCharacter): return typeof(UnityEngine.Animator);
                default: return null;
            }
        }

        private string GetTooltip ()
        {
            if (AllowMultipleResources)
                return $"Use `@char {EditedActorId}.%name%` in novel scripts to show the character with selected appearance.";
            return $"Use `@char {EditedActorId}` in novel scripts to show this character.";
        }

        private Dictionary<string, Action<SerializedProperty>> GetOverrideConfigurationDrawers ()
        {
            var overrideConfigurationDrawers = base.OverrideConfigurationDrawers;
            overrideConfigurationDrawers["AvatarLoader"] = DrawAvatarsEditor;
            return overrideConfigurationDrawers;
        }

        private void DrawAvatarsEditor (SerializedProperty avatarsLoaderProperty)
        {
            EditorGUILayout.PropertyField(avatarsLoaderProperty);

            avatarsEditorExpanded = EditorGUILayout.Foldout(avatarsEditorExpanded, AvatarsEditorContent, true);
            if (!avatarsEditorExpanded) return;
            ResourcesEditor.DrawGUILayout(Configuration.AvatarLoader.PathPrefix, Configuration.AvatarLoader.PathPrefix, null, typeof(Texture2D), 
                "Use `@char CharacterID avatar:%name% in novel scripts to assign selected avatar texture for the character.`");
        }

        [MenuItem("Naninovel/Resources/Characters")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
