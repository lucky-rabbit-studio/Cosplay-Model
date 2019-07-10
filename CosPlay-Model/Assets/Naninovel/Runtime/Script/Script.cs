// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Representation of a text file used to author naninovel script flow.
    /// </summary>
    public class Script
    {
        public readonly string Name;
        public readonly List<ScriptLine> Lines;
        public readonly List<CommandScriptLine> CommandLines;
        public readonly List<CommentScriptLine> CommentLines;
        public readonly List<DefineScriptLine> DefineLines;
        public readonly List<GenericTextScriptLine> GenericTextLines;
        public readonly List<LabelScriptLine> LabelLines;

        /// <summary>
        /// Creates new instance from serialized script text.
        /// </summary>
        public Script (string scriptName, string scriptText, List<DefineScriptLine> globalDefines = null, bool ignoreErrors = false)
            : this(scriptName, ParseScriptText(scriptName, scriptText, globalDefines, ignoreErrors)) { }

        /// <summary>
        /// Creates new instance from a list of <see cref="ScriptLine"/>.
        /// </summary>
        public Script (string scriptName, List<ScriptLine> scriptLines)
        {
            Name = scriptName;
            Lines = scriptLines;
            CommandLines = Lines.OfType<CommandScriptLine>().ToList();
            CommentLines = Lines.OfType<CommentScriptLine>().ToList();
            DefineLines = Lines.OfType<DefineScriptLine>().ToList();
            GenericTextLines = Lines.OfType<GenericTextScriptLine>().ToList();
            LabelLines = Lines.OfType<LabelScriptLine>().ToList();
        }

        public static Type ResolveLineType (string lineText)
        {
            if (string.IsNullOrWhiteSpace(lineText))
                return typeof(CommentScriptLine);
            else if (lineText.StartsWithFast(CommandScriptLine.IdentifierLiteral))
                return typeof(CommandScriptLine);
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
        /// Returns list of all the <see cref="CommandScriptLine"/>, including the ones inlined in <see cref="GenericTextScriptLine"/>.
        /// The order of the commands will be retained.
        /// </summary>
        public List<CommandScriptLine> CollectAllCommandLines ()
        {
            var result = new List<CommandScriptLine>();
            foreach (var line in Lines)
            {
                switch (line)
                {
                    case CommandScriptLine commandLine:
                        result.Add(commandLine);
                        break;
                    case GenericTextScriptLine genericLine:
                        result.AddRange(genericLine.InlinedCommandLines);
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
        /// Returns number of <see cref="CommandScriptLine"/> at the provided line index.
        /// </summary>
        public int CountCommandsAtLine (int lineIndex)
        {
            if (!IsLineIndexValid(lineIndex)) return 0;
            var line = Lines[lineIndex];
            switch (line)
            {
                case CommandScriptLine _:
                    return 1;
                case GenericTextScriptLine genericLine:
                    return genericLine.InlinedCommandLines?.Count ?? 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Checks whether provided inline index of a <see cref="CommandScriptLine"/> is last at the provided line index.
        /// </summary>
        public bool IsCommandFinalAtLine (int lineIndex, int inlineIndex)
        {
            var finalIndex = CountCommandsAtLine(lineIndex) - 1;
            return inlineIndex == finalIndex;
        }

        /// <summary>
        /// Checks whether a <see cref="CommandScriptLine"/> exists at the provided indexes.
        /// </summary>
        public bool IsCommandIndexValid (int lineIndex, int inlineIndex)
        {
            var inlineCount = CountCommandsAtLine(lineIndex);
            return inlineIndex < inlineCount;
        }

        private static List<ScriptLine> ParseScriptText (string scriptName, string scriptText, List<DefineScriptLine> globalDefines, bool ignoreErrors)
        {
            var scriptDefines = new LiteralMap<string>();
            if (globalDefines != null && globalDefines.Count > 0)
            {
                foreach (var define in globalDefines)
                    if (!string.IsNullOrEmpty(define.DefineKey) && !string.IsNullOrEmpty(define.DefineValue))
                        scriptDefines[define.DefineKey] = define.DefineValue;
            }

            var scriptLines = new List<ScriptLine>();
            var scriptLinesText = scriptText?.TrimFull()?.SplitByNewLine() ?? new[] { string.Empty };
            for (int i = 0; i < scriptLinesText.Length; i++)
            {
                var scriptLineText = scriptLinesText[i].TrimFull();
                var scriptLineType = ResolveLineType(scriptLineText);
                var scriptLine = Activator.CreateInstance(scriptLineType, scriptName, i, scriptLineText, scriptDefines, ignoreErrors) as ScriptLine;
                scriptLines.Add(scriptLine);
            }
            return scriptLines;
        }
    }
}
