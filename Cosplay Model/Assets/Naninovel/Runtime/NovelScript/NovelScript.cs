// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Representation of a text file used to author novel script flow.
    /// </summary>
    public class NovelScript
    {
        public string Name { get; }
        public List<NovelScriptLine> Lines { get; }
        public List<ActionScriptLine> ActionLines { get; }
        public List<CommentScriptLine> CommentLines { get; }
        public List<DefineScriptLine> DefineLines { get; }
        public List<GenericTextScriptLine> GenericTextLines { get; }
        public List<LabelScriptLine> LabelLines { get; }

        /// <summary>
        /// Creates new instance from serialized script text.
        /// </summary>
        public NovelScript (string scriptName, string scriptText)
            : this(scriptName, ParseScriptText(scriptName, scriptText)) { }

        /// <summary>
        /// Creates new instance from a list of <see cref="NovelScriptLine"/>.
        /// </summary>
        public NovelScript (string scriptName, List<NovelScriptLine> scriptLines)
        {
            Name = scriptName;
            Lines = scriptLines;
            ActionLines = Lines.OfType<ActionScriptLine>().ToList();
            CommentLines = Lines.OfType<CommentScriptLine>().ToList();
            DefineLines = Lines.OfType<DefineScriptLine>().ToList();
            GenericTextLines = Lines.OfType<GenericTextScriptLine>().ToList();
            LabelLines = Lines.OfType<LabelScriptLine>().ToList();
        }

        public static Type ResolveLineType (string lineText)
        {
            if (string.IsNullOrWhiteSpace(lineText))
                return typeof(CommentScriptLine);
            else if (lineText.StartsWithFast(ActionScriptLine.IdentifierLiteral))
                return typeof(ActionScriptLine);
            else if (lineText.StartsWithFast(CommentScriptLine.IdentifierLiteral))
                return typeof(CommentScriptLine);
            else if (lineText.StartsWithFast(LabelScriptLine.IdentifierLiteral))
                return typeof(LabelScriptLine);
            else if (lineText.StartsWithFast(DefineScriptLine.IdentifierLiteral))
                return typeof(DefineScriptLine);
            else return typeof(GenericTextScriptLine);
        }

        public bool IsLineIndexValid (int lineIndex)
        {
            return lineIndex >= 0 && Lines.Count > lineIndex;
        }

        public bool LabelExists (string label)
        {
            return LabelLines.Exists(l => l.LabelText.EqualsFastIgnoreCase(label));
        }

        public int GetLineIndexForLabel (string label)
        {
            if (!LabelExists(label)) return -1;
            else return LabelLines.Find(l => l.LabelText.EqualsFastIgnoreCase(label)).LineIndex;
        }

        /// <summary>
        /// Returns list of all the <see cref="ActionScriptLine"/>, including the ones inlined in <see cref="GenericTextScriptLine"/>.
        /// The order of the actions will be retained.
        /// </summary>
        public List<ActionScriptLine> CollectAllActionLines ()
        {
            var result = new List<ActionScriptLine>();
            foreach (var line in Lines)
            {
                switch (line)
                {
                    case ActionScriptLine actionLine:
                        result.Add(actionLine);
                        break;
                    case GenericTextScriptLine genericLine:
                        result.AddRange(genericLine.InlinedActionLines);
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// Returns <see cref="CommentScriptLine.CommentText"/> of the <see cref="CommentScriptLine"/> located before line with the provided index.
        /// </summary>
        public string GetCommentForLine (int lineIndex)
        {
            if (!IsLineIndexValid(lineIndex)) return null;
            var commentLine = Lines[lineIndex] as CommentScriptLine;
            return commentLine?.CommentText;
        }

        /// <summary>
        /// Returns number of <see cref="ActionScriptLine"/> at the provided line index.
        /// </summary>
        public int CountActionAtLine (int lineIndex)
        {
            if (!IsLineIndexValid(lineIndex)) return 0;
            var line = Lines[lineIndex];
            switch (line)
            {
                case ActionScriptLine actionLine:
                    return 1;
                case GenericTextScriptLine genericLine:
                    return genericLine.InlinedActionLines?.Count ?? 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Checks whether provided inline index of a <see cref="ActionScriptLine"/> is last at the provided line index.
        /// </summary>
        public bool IsActionFinalAtLine (int lineIndex, int inlineIndex)
        {
            var finalIndex = CountActionAtLine(lineIndex) - 1;
            return inlineIndex == finalIndex;
        }

        /// <summary>
        /// Checks whether a <see cref="ActionScriptLine"/> exists at the provided indexes.
        /// </summary>
        public bool IsActionIndexValid (int lineIndex, int inlineIndex)
        {
            var inlineCount = CountActionAtLine(lineIndex);
            return inlineIndex < inlineCount;
        }

        private static List<NovelScriptLine> ParseScriptText (string scriptName, string scriptText)
        {
            var scriptDefines = new LiteralMap<string>();
            var scriptLines = new List<NovelScriptLine>();
            var scriptLinesText = scriptText?.TrimFull()?.SplitByNewLine() ?? new[] { string.Empty };
            for (int i = 0; i < scriptLinesText.Length; i++)
            {
                var scriptLineText = scriptLinesText[i].TrimFull();
                var scriptLineType = ResolveLineType(scriptLineText);
                var scriptLine = Activator.CreateInstance(scriptLineType, scriptName, i, scriptLineText, scriptDefines) as NovelScriptLine;
                scriptLines.Add(scriptLine);
            }
            return scriptLines;
        }
    }
}
