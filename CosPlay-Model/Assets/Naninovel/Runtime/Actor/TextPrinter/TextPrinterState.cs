// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="ITextPrinterActor"/>.
    /// </summary>
    [System.Serializable]
    public class TextPrinterState : ActorState<ITextPrinterActor>
    {
        public bool IsPrinterActive = false;
        public string PrintedText = null;
        public string AuthorId = null;
        public float PrintDelay = .03f;
        public List<string> ActiveRichTextTags = new List<string>();

        public override void ApplyToActor (ITextPrinterActor actor)
        {
            base.ApplyToActor(actor);
            actor.IsPrinterActive = IsPrinterActive;
            actor.PrintedText = PrintedText;
            actor.AuthorId = AuthorId;
            actor.PrintDelay = PrintDelay;
            actor.RichTextTags = new List<string>(ActiveRichTextTags);
        }

        public override void OverwriteFromActor (ITextPrinterActor actor)
        {
            base.OverwriteFromActor(actor);
            IsPrinterActive = actor.IsPrinterActive;
            PrintedText = actor.PrintedText;
            AuthorId = actor.AuthorId;
            PrintDelay = actor.PrintDelay;
            ActiveRichTextTags = new List<string>(actor.RichTextTags);
        }
    }
}
