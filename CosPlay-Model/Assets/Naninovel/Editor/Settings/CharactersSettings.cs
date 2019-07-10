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
            [nameof(CharacterMetadata.NameColor)] = property => { if (EditedMetadata.UseCharacterColor) EditorGUILayout.PropertyField(property); },
            [nameof(CharacterMetadata.MessageColor)] = property => { if (EditedMetadata.UseCharacterColor) EditorGUILayout.PropertyField(property); },
            [nameof(CharacterMetadata.SpeakingTint)] = property => { if (EditedMetadata.HighlightWhenSpeaking) EditorGUILayout.PropertyField(property); },
            [nameof(CharacterMetadata.NotSpeakingTint)] = property => { if (EditedMetadata.HighlightWhenSpeaking) EditorGUILayout.PropertyField(property); },
            [nameof(CharacterMetadata.MessageSound)] = property => EditorResources.DrawPathPopup(property, AudioConfiguration.DefaultAudioPathPrefix, AudioConfiguration.DefaultAudioPathPrefix, "None (disabled)"),
        };

        private bool avatarsEditorExpanded;

        private Type GetTypeConstraint ()
        {
            switch (EditedMetadata?.Implementation?.GetAfter("."))
            {
                case nameof(SpriteCharacter): return typeof(UnityEngine.Texture2D);
                case nameof(GenericCharacter): return typeof(CharacterActorBehaviour);
                default: return null;
            }
        }

        private string GetTooltip ()
        {
            if (AllowMultipleResources)
                return $"Use `@char {EditedActorId}.%name%` in naninovel scripts to show the character with selected appearance.";
            return $"Use `@char {EditedActorId}` in naninovel scripts to show this character.";
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
                "Use `@char CharacterID avatar:%name% in naninovel scripts to assign selected avatar texture for the character.`");
        }

        [MenuItem("Naninovel/Resources/Characters")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
