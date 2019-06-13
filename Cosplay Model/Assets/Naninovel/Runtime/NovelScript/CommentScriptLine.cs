// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="NovelScript"/> line representing a commentary left by the author of the script.
    /// </summary>
    public class CommentScriptLine : NovelScriptLine
    {
        /// <summary>
        /// Literal used to identify this type of lines.
        /// </summary>
        public const string IdentifierLiteral = ";";
        /// <summary>
        /// Text contents of the commentary (trimmed string after the `;` symbol).
        /// </summary>
        public string CommentText { get; }

        public CommentScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null) 
            : base(scriptName, lineIndex, lineText, scriptDefines)
        {
            CommentText = ParseComment(Text);
        }

        private static string ParseComment (string lineText) => lineText?.GetAfter(IdentifierLiteral)?.TrimFull();
    }
}
