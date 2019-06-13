// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.UI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="ITextPrinterActor"/> implementation using <see cref="UITextPrinterPanel"/> to represent an actor.
    /// </summary>
    public class UITextPrinter : MonoBehaviourActor, ITextPrinterActor
    {
        public override string Appearance { get; set; }
        public override bool IsVisible { get => PrinterPanel.IsVisible; set => PrinterPanel.IsVisible = value; }
        public bool IsPrinterActive { get => isPrinterActive; set => SetIsPrinterActive(value); }
        public string PrintedText { get => PrinterPanel.PrintedText; set => SetPrintedText(value); }
        public string LastPrintedText { get; private set; }
        public string AuthorId { get => authorId; set => SetAuthorId(value); }
        public float PrintDelay { get; set; }
        public List<string> RichTextTags { get => richTextTags; set => SetRichTextTags(value); }

        protected UITextPrinterPanel PrinterPanel { get; private set; }
        protected bool IsUsingRichTags => RichTextTags?.Count > 0;

        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private string authorId;
        private TextPrinterMetadata metadata;
        private bool isPrinterActive = false;
        private List<string> richTextTags = new List<string>();
        private CancellationTokenSource revealTextCTS;
        private InputManager inputManager;
        private NovelScriptPlayer scriptPlayer;
        private CharacterManager characterManager;
        private string activeTagsOpenSequence;
        private string activeTagsCloseSequence;

        public UITextPrinter (string id, TextPrinterMetadata metadata)
            : base(id, metadata)
        {
            this.metadata = metadata;
            inputManager = Engine.GetService<InputManager>();
            scriptPlayer = Engine.GetService<NovelScriptPlayer>();
            characterManager = Engine.GetService<CharacterManager>();
            activeTagsOpenSequence = string.Empty;
            activeTagsCloseSequence = string.Empty;
        }

        public override async Task InitializeAsync ()
        {
            await base.InitializeAsync();

            var providerMngr = Engine.GetService<ResourceProviderManager>();
            var prefabResource = await metadata.LoaderConfiguration.CreateFor<GameObject>(providerMngr).LoadAsync(Id);
            if (!prefabResource.IsValid)
            {
                Debug.LogError($"Failed to load `{Id}` UI text printer resource object. Make sure the printer is correctly configured.");
                return;
            }

            var uiMngr = Engine.GetService<UIManager>();
            PrinterPanel = uiMngr.InstantiateUIPrefab(prefabResource.Object) as UITextPrinterPanel;
            PrinterPanel.transform.SetParent(Transform);

            await PrinterPanel.InitializeAsync();

            inputManager.Continue.AddObjectTrigger(PrinterPanel.ContinueInputTrigger);
            scriptPlayer.OnWaitingForInput += PrinterPanel.SetWaitForInputIndicatorVisible;

            SetAuthorId(null);
            ResetText();
            IsVisible = false;
        }

        public override Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default)
        {
            throw new System.NotImplementedException();
        }

        public override async Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default)
        {
            await PrinterPanel?.SetIsVisibleAsync(isVisible, duration);
        }

        public virtual async Task PrintTextAsync (string text, string actorName, params CancellationToken[] cancellationTokens)
        {
            CancelRevealTextRoutine();

            SetAuthorId(actorName);
            AppendText(text);
            LastPrintedText = text;
            if (!IsVisible) PrinterPanel.Show();

            if (!scriptPlayer.IsSkipActive && PrintDelay > 0)
            {
                revealTextCTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens);
                await ActorBehaviour.StartCoroutine(PrinterPanel.RevealPrintedTextOverTime(revealTextCTS.Token, PrintDelay));
            }
            PrinterPanel?.RevealPrintedText();

            await waitForEndOfFrame;
        }

        public void ResetText ()
        {
            CancelRevealTextRoutine();
            PrinterPanel.HidePrintedText();
            PrinterPanel.PrintedText = IsUsingRichTags ? activeTagsOpenSequence + activeTagsCloseSequence : string.Empty;
        }

        public void AppendText (string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend)) return;

            var insertIndex = PrinterPanel.PrintedText.Length - activeTagsCloseSequence.Length;
            PrinterPanel.PrintedText = PrinterPanel.PrintedText.Insert(insertIndex, textToAppend);
        }

        public void SetRichTextTags (List<string> tags)
        {
            if (tags is null) richTextTags.Clear();
            else richTextTags = tags;

            if (IsUsingRichTags)
            {
                activeTagsOpenSequence = GetActiveTagsOpenSequence();
                activeTagsCloseSequence = GetActiveTagsCloseSequence();
                PrinterPanel.PrintedText += activeTagsOpenSequence + activeTagsCloseSequence;
            }
            else
            {
                activeTagsOpenSequence = string.Empty;
                activeTagsCloseSequence = string.Empty;
            }
        }

        public void SetAuthorId (string authorId)
        {
            this.authorId = authorId;

            // Attempt to find a character display name for the provided actor ID.
            var displayName = characterManager.GetDisplayName(authorId) ?? authorId;
            PrinterPanel.ActorNameText = displayName;

            // Update author meta.
            var authorMeta = characterManager.GetActorMetadata<CharacterMetadata>(authorId);
            PrinterPanel.OnAuthorChanged(authorId, authorMeta);
        }

        public override Task PreloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        public override Task UnloadResourcesAsync (string appearance = null) => Task.CompletedTask;

        public override void Dispose ()
        {
            base.Dispose();

            inputManager.Continue.RemoveObjectTrigger(PrinterPanel.ContinueInputTrigger);
            scriptPlayer.OnWaitingForInput -= PrinterPanel.SetWaitForInputIndicatorVisible;
            PrinterPanel = null;
            CancelRevealTextRoutine();
        }

        protected override void SetBehaviourPosition (Vector3 position)
        {
            // Changing transform of the root obj won't work; modify content panel instead.
            if (!PrinterPanel || !PrinterPanel.Content) return;
            PrinterPanel.Content.localPosition = position;
        }

        protected override void SetBehaviourRotation (Quaternion rotation)
        {
            if (!PrinterPanel || !PrinterPanel.Content) return;
            PrinterPanel.Content.localRotation = rotation;
        }

        protected override void SetBehaviourScale (Vector3 scale)
        {
            if (!PrinterPanel || !PrinterPanel.Content) return;
            PrinterPanel.Content.localScale = scale;
        }

        protected override Color GetBehaviourTintColor () => Color.white;

        protected override void SetBehaviourTintColor (Color tintColor) { }

        protected virtual void SetPrintedText (string text)
        {
            PrinterPanel.PrintedText = text;
            PrinterPanel.RevealPrintedText();
        }

        private void CancelRevealTextRoutine ()
        {
            revealTextCTS?.Cancel();
            revealTextCTS?.Dispose();
            revealTextCTS = null;
        }

        private void SetIsPrinterActive (bool isPrinterActive)
        {
            this.isPrinterActive = isPrinterActive;
            if (!isPrinterActive)
            {
                PrinterPanel?.Hide();
                CancelRevealTextRoutine();
            }
        }

        private string GetActiveTagsOpenSequence ()
        {
            var result = string.Empty;

            if (RichTextTags is null || RichTextTags.Count == 0)
                return result;

            foreach (var tag in RichTextTags)
                result += string.Format("<{0}>", tag);

            return result;
        }

        private string GetActiveTagsCloseSequence ()
        {
            var result = string.Empty;

            if (RichTextTags is null || RichTextTags.Count == 0)
                return result;

            var reversedActiveTags = RichTextTags;
            reversedActiveTags.Reverse();
            foreach (var tag in reversedActiveTags)
                result += string.Format("</{0}>", tag.GetBefore("=") ?? tag);

            return result;
        }
    } 
}
