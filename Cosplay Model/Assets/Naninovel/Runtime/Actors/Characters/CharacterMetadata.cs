// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="ICharacterActor"/>.
    /// </summary>
    [System.Serializable]
    public class CharacterMetadata : OrthoActorMetadata
    {
        [System.Serializable]
        public class Map : NovelActorMetadataMap<CharacterMetadata> { }

        [Tooltip("Full name of the character to display in printer name label UI. Will use character ID when not specified.")]
        public string DisplayName = default;
        [Tooltip("Whether to apply character-specific color to printer messages and name label UI.")]
        public bool UseCharacterColor = false;
        [Tooltip("Character-specific color to tint printer name label UI.")]
        public Color NameColor = Color.white;
        [Tooltip("Character-specific color to tint printer messages.")]
        public Color MessageColor = Color.white;
        [Tooltip("When enabled, will apply specified tint colors based on whether this actor is the author of the last printed text.")]
        public bool HighlightWhenSpeaking = false;
        [Tooltip("Tint color to apply when the character is speaking.")]
        public Color SpeakingTint = Color.white;
        [Tooltip("Tint color to apply when the character is not speaking.")]
        public Color NotSpeakingTint = Color.gray;

        public CharacterMetadata ()
        {
            Implementation = typeof(SpriteCharacter).FullName;
            LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = CharactersConfiguration.DefaultCharactersPathPrefix };
            Pivot = new Vector2(.5f, .0f);
        }
    }
}
