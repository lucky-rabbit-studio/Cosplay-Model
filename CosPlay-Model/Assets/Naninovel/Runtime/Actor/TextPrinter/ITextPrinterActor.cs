// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a text printer actor on scene.
    /// </summary>
    public interface ITextPrinterActor : IActor
    {
        /// <summary>
        /// Whether the printer should handle print tasks by default.
        /// </summary>
        bool IsPrinterActive { get; set; }
        /// <summary>
        /// The text which the printer has currently printed.
        /// </summary>
        string PrintedText { get; set; }
        /// <summary>
        /// Returns text that was printed at the last <see cref="PrintTextAsync(string, string, CancellationToken[])"/> invocation.
        /// </summary>
        string LastPrintedText { get; }
        /// <summary>
        /// ID of the actor to which currently printed text belongs.
        /// </summary>
        string AuthorId { get; set; }
        /// <summary>
        /// Delay (in seconds) after each printed text character. Lower the value, faster the printing speed.
        /// </summary>
        float PrintDelay { get; set; }
        /// <summary>
        /// Currently active rich text tags.
        /// </summary>
        List<string> RichTextTags { get; set; }

        /// <summary>
        /// Outputs the provided text over time, gradually revealing characters one by one.
        /// </summary>
        /// <param name="text">The text to print.</param>
        /// <param name="actorName">Name (ID) of the actor to whom the text belongs.</param>
        /// <param name="cancellationTokens">Tokens for task concellation.</param>
        Task PrintTextAsync (string text, string actorName, params CancellationToken[] cancellationTokens);
        /// <summary>
        /// Adds text to the current output.
        /// </summary>
        void AppendText (string text);
        /// <summary>
        /// Clears printed text.
        /// </summary>
        void ResetText ();
    } 
}
