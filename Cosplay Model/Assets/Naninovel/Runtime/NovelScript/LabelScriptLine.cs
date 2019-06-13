// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="NovelScript"/> line representing a text marker used to navigate within the script.
    /// </summary>
    public class LabelScriptLine : NovelScriptLine
    {
        /// <summary>
        /// Literal used to identify this type of lines.
        /// </summary>
        public const string IdentifierLiteral = "#";
        /// <summary>
        /// Text contents of the label (trimmed string after the <see cref="IdentifierLiteral"/>).
        /// </summary>
        public string LabelText { get; }

        public LabelScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null) 
            : base(scriptName, lineIndex, lineText, scriptDefines)
        {
            LabelText = ParseLabel(Text);
        }

        private static string ParseLabel (string lineText) => lineText?.GetAfter(IdentifierLiteral)?.TrimFull();
    }
}
