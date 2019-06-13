// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    /// <summary>
    /// A <see cref="UITextPrinterPanel"/> implementation that uses <see cref="IRevealableText"/> to reveal text over time.
    /// </summary>
    /// <remarks>
    /// A <see cref="IRevealableText"/> component should be attached to the underlying gameobject or one of it's child objects.
    /// </remarks>
    public class RevealableTextPrinterPanel : UITextPrinterPanel
    {
        [System.Serializable]
        private class CharsToSfx
        {
            [Tooltip("The characters for which to trigger the SFX.")]
            public string Characters = default;
            [ResourcesPopup(AudioConfiguration.DefaultAudioPathPrefix, AudioConfiguration.DefaultAudioPathPrefix, "None (disabled)")]
            [Tooltip("The name (local path) of the SFX to trigger for the specified characters.")]
            public string SfxName = default;
        }

        public override string PrintedText { get => RevealableText.Text; set => RevealableText.Text = value; }
        public override string ActorNameText { get => actorNamePanel ? actorNamePanel.Text : null; set => SetActorNameText(value); }
        public IRevealableText RevealableText { get; private set; }

        protected string AuthorId { get; private set; }
        protected CharacterMetadata AuthorMeta { get; private set; }

        [SerializeField] private ActorNamePanel actorNamePanel = default;
        [SerializeField] private RawImage actorAvatarImage = default;
        [SerializeField] private WaitingForInputIndicator inputIndicatorPrefab = default;
        [ResourcesPopup(AudioConfiguration.DefaultAudioPathPrefix, AudioConfiguration.DefaultAudioPathPrefix, "None (disabled)")]
        [Tooltip ("If specified, SFX with the provided name (local path) will be played whenever a character is revealed.")]
        [SerializeField] private string RevealSfx = default;
        [Tooltip("Here you can bind an SFX to play when specific characters are revealed.")]
        [SerializeField] private List<CharsToSfx> charsSfx = new List<CharsToSfx>();

        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private Color defaultMessageColor, defaultNameColor;
        private WaitingForInputIndicator inputIndicator;
        private AudioManager audioManager;

        public override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            if (!string.IsNullOrEmpty(RevealSfx))
                await audioManager.PreloadAudioAsync(RevealSfx);
            if (charsSfx != null && charsSfx.Count > 0)
            {
                var loadTasks = new List<Task>();
                foreach (var charSfx in charsSfx)
                    if (!string.IsNullOrEmpty(charSfx.SfxName))
                        loadTasks.Add(audioManager.PreloadAudioAsync(charSfx.SfxName));
                await Task.WhenAll(loadTasks);
            }
        }

        public override IEnumerator RevealPrintedTextOverTime (CancellationToken cancellationToken, float revealDelay)
        {
            if (revealDelay <= 0) { RevealableText.RevealAll(); yield break; }

            var timeSinceLastReveal = 0f;
            while (!RevealableText.IsFullyRevealed)
            {
                var charsToReveal = Mathf.FloorToInt(timeSinceLastReveal / revealDelay);
                if (charsToReveal > 0)
                {
                    timeSinceLastReveal = timeSinceLastReveal % revealDelay;
                    for (int i = 0; i < charsToReveal; i++)
                        RevealableText.RevealNextChar(revealDelay);
                    PlayRevealSfxFor(RevealableText.GetLastRevealedChar());
                }

                yield return waitForEndOfFrame;
                timeSinceLastReveal += Time.deltaTime;
                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        public override void RevealPrintedText () => RevealableText.RevealAll();

        public override void HidePrintedText () => RevealableText.HideAll();

        public override async void SetWaitForInputIndicatorVisible (bool isVisible)
        {
            await waitForEndOfFrame; // Otherwise cachedTextGenerator and inidicator position may be invalid.

            if (isVisible) inputIndicator.Show(RevealableText.GetLastRevealedCharPosition());
            else inputIndicator.Hide();
        }

        public override void OnAuthorChanged (string authorId, CharacterMetadata authorMeta)
        {
            AuthorId = authorId;
            AuthorMeta = authorMeta;

            // Attempt to apply character-specific message text color.
            RevealableText.TextColor = authorMeta.UseCharacterColor ? authorMeta.MessageColor : defaultMessageColor;

            // Attempt to set character name color.
            if (actorNamePanel)
            {
                actorNamePanel.TextColor = authorMeta.UseCharacterColor ? authorMeta.NameColor : defaultNameColor;
            }

            // Attempt to set character-specific avatar texture.
            if (actorAvatarImage)
            {
                var avatarTexture = CharacterManager.GetAvatarTextureFor(authorId);
                actorAvatarImage.gameObject.SetActive(avatarTexture);
                actorAvatarImage.texture = avatarTexture;
            }
        }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(inputIndicatorPrefab);

            RevealableText = GetComponentInChildren<IRevealableText>();
            Debug.Assert(RevealableText != null, $"IRevealableText component not found on {gameObject.name} or it's descendants.");

            defaultMessageColor = RevealableText.TextColor;
            defaultNameColor = actorNamePanel ? actorNamePanel.TextColor : default;

            inputIndicator = Instantiate(inputIndicatorPrefab);
            inputIndicator.RectTransform.SetParent(RevealableText.GameObject.transform, false);

            audioManager = Engine.GetService<AudioManager>();

            SetActorNameText(null); // Reset the name-related stuff.
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            CharacterManager.OnCharacterAvatarChanged += HandleAvatarChanged;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            CharacterManager.OnCharacterAvatarChanged -= HandleAvatarChanged;
        }

        protected virtual void SetActorNameText (string text)
        {
            if (!actorNamePanel) return;

            var isActive = !string.IsNullOrWhiteSpace(text);
            actorNamePanel.gameObject.SetActive(isActive);
            if (!isActive) return;

            actorNamePanel.Text = text;
        }

        protected virtual void HandleAvatarChanged (CharacterAvatarChangedArgs args)
        {
            if (!actorAvatarImage) return;

            actorAvatarImage.gameObject.SetActive(args.AvatarTexture);
            actorAvatarImage.texture = args.AvatarTexture;
        }

        protected virtual void PlayRevealSfxFor (char character)
        {
            if (charsSfx != null && charsSfx.Count > 0)
            {
                foreach (var charSfx in charsSfx)
                {
                    var index = charSfx.Characters.IndexOf(character);
                    if (index < 0) continue;

                    if (!string.IsNullOrEmpty(charSfx.SfxName))
                        audioManager.PlaySfxFast(charSfx.SfxName);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(RevealSfx))
                audioManager.PlaySfxFast(RevealSfx);
        }
    }
}
