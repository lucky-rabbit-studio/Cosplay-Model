// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Used by <see cref="UITextPrinter"/> to control the printed text.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UITextPrinterPanel : ScriptableUIBehaviour, IManagedUI
    {
        /// <summary>
        /// Contents of the printer to be used for transformations.
        /// </summary>
        public RectTransform Content => content;
        /// <summary>
        /// The text to be printed inside the printer panel. 
        /// Note that the visibility of the text is controlled independently.
        /// </summary>
        public abstract string PrintedText { get; set; }
        /// <summary>
        /// Text representing name of the author of the currently printed text.
        /// </summary>
        public abstract string ActorNameText { get; set; }
        /// <summary>
        /// Object that should trigger continue input when interacted with.
        /// </summary>
        public GameObject ContinueInputTrigger => continueInputTrigger;

        protected RectTransform PrinterContent => content;
        protected CharacterManager CharacterManager { get; private set; }

        [Tooltip("Transform used for printer position, scale and rotation external manipulations.")]
        [SerializeField] private RectTransform content = default;
        [Tooltip("Object that should trigger continue input when interacted with. Make sure the object is a raycast target and is not blocked by other raycast target objects.")]
        [SerializeField] private GameObject continueInputTrigger = default;

        public virtual Task InitializeAsync () => Task.CompletedTask;

        /// <summary>
        /// A coroutine to reveal the <see cref="PrintedText"/> char by char over time.
        /// </summary>
        /// <param name="cancellationToken">The coroutine will break when cancellation of the provided token is requested.</param>
        /// <param name="revealDelay">Delay (in seconds) between revealing consequent characters.</param>
        /// <returns>Coroutine enumerator.</returns>
        public abstract IEnumerator RevealPrintedTextOverTime (CancellationToken cancellationToken, float revealDelay);
        /// <summary>
        /// Instantly reveals all the <see cref="PrintedText"/>.
        /// </summary>
        public abstract void RevealPrintedText ();
        /// <summary>
        /// Instantly hides all the <see cref="PrintedText"/>.
        /// </summary>
        public abstract void HidePrintedText ();
        /// <summary>
        /// Controls visibility of the wait for input indicator.
        /// </summary>
        public abstract void SetWaitForInputIndicatorVisible (bool isVisible);
        /// <summary>
        /// Invoked by <see cref="UITextPrinter"/> when author meta of the printed text changes.
        /// </summary>
        /// <param name="authorId">Acotr ID of the new author.</param>
        /// <param name="authorMeta">Metadata of the new author.</param>
        public abstract void OnAuthorChanged (string authorId, CharacterMetadata authorMeta);

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(content, continueInputTrigger);

            CharacterManager = Engine.GetService<CharacterManager>();
        }
    } 
}
