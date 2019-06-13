// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="NovelScript"/> line representing text to print.
    /// Could contain actor name at the start of the line followed by a column,
    /// and multiple inlined <see cref="ActionScriptLine"/> enclosed in square brackets.
    /// </summary>
    public class GenericTextScriptLine : NovelScriptLine
    {
        /// <summary>
        /// Literal used to declare actor name before the text to print.
        /// </summary>
        public const string ActorNameLiteral = ": ";
        /// <summary>
        /// A list of <see cref="ActionScriptLine"/> inlined in this line.
        /// </summary>
        public List<ActionScriptLine> InlinedActionLines { get; }

        public GenericTextScriptLine (string scriptName, int lineNumber, string lineText, LiteralMap<string> scriptDefines = null) 
            : base(scriptName, lineNumber, lineText, scriptDefines)
        {
            InlinedActionLines = ExtractInlinedActionLines(Text, scriptDefines);
        }

        public GenericTextScriptLine (string scriptName, int lineNumber, List<ActionScriptLine> inlinedActionLines) 
            : base(scriptName, lineNumber, string.Empty)
        {
            InlinedActionLines = inlinedActionLines;
        }

        protected override string ReplaceDefines (string lineText, LiteralMap<string> defines)
        {
            foreach (var define in defines) // Actor names in generic text lines doesn't require replace literal to be replaced.
                if (lineText.StartsWithFast($"{define.Key}: ")) { lineText = lineText.Replace($"{define.Key}: ", $"{define.Value}: "); break; }

            return base.ReplaceDefines(lineText, defines);
        }

        private List<ActionScriptLine> ExtractInlinedActionLines (string lineText, LiteralMap<string> scriptDefines = null)
        {
            // When actor name is present at the start of the line: extract it and cut from the line.
            var actorName = lineText.GetBefore(ActorNameLiteral);
            if (!string.IsNullOrEmpty(actorName) && !actorName.Any(char.IsWhiteSpace) && !actorName.StartsWithFast("\""))
                lineText = lineText.GetAfterFirst(ActorNameLiteral);
            else actorName = null;

            // Collect all inlined action strings (text inside square brackets).
            var inlinedActionMatches = Regex.Matches(lineText, "\\[.*?\\]").Cast<Match>().ToList();

            // In case no inlined actions found, just add a single @print action line.
            if (inlinedActionMatches.Count == 0)
            {
                var printLineText = TransformGenericToPrintText(lineText, actorName);
                var printLine = new ActionScriptLine(ScriptName, LineIndex, printLineText, scriptDefines);
                return new List<ActionScriptLine> { printLine };
            }

            var result = new List<ActionScriptLine>();
            var printedTextBefore = false;
            for (int i = 0; i < inlinedActionMatches.Count; i++)
            {
                // Check if we need to print any text before the current inlined action.
                var precedingGenericText = StringUtils.TrySubset(lineText,
                    i > 0 ? inlinedActionMatches[i - 1].GetEndIndex() + 1 : 0,
                    inlinedActionMatches[i].Index - 1);
                if (!string.IsNullOrEmpty(precedingGenericText))
                {
                    var printLineText = TransformGenericToPrintText(precedingGenericText, actorName, printedTextBefore ? (bool?)false : null, false);
                    var printLine = new ActionScriptLine(ScriptName, LineIndex, printLineText, scriptDefines, result.Count);
                    result.Add(printLine);
                    printedTextBefore = true;
                }

                // Extract inlined action script line.
                var actionLineText = ActionScriptLine.IdentifierLiteral + inlinedActionMatches[i].ToString().GetBetween("[", "]").TrimFull();
                var actionLine = new ActionScriptLine(ScriptName, LineIndex, actionLineText, scriptDefines, result.Count);
                result.Add(actionLine);
            }

            // Check if we need to print any text after the last inlined action.
            var lastGenericText = StringUtils.TrySubset(lineText,
                     inlinedActionMatches.Last().GetEndIndex() + 1,
                     lineText.Length - 1);
            if (!string.IsNullOrEmpty(lastGenericText))
            {
                var printLineText = TransformGenericToPrintText(lastGenericText, actorName, printedTextBefore ? (bool?)false : null, false);
                var printLine = new ActionScriptLine(ScriptName, LineIndex, printLineText, scriptDefines, result.Count);
                result.Add(printLine);
            }

            // Add wait input action at the end.
            var waitActionLineText = ActionScriptLine.IdentifierLiteral + typeof(WaitForInput).Name;
            var waitActionLine = new ActionScriptLine(ScriptName, LineIndex, waitActionLineText, scriptDefines, result.Count);
            result.Add(waitActionLine);

            return result;
        }

        /// <summary>
        /// Transforms a generic text string to print action line text, which can be used 
        /// to create an <see cref="ActionScriptLine"/> for <see cref="PrintText"/> action.
        /// </summary>
        private static string TransformGenericToPrintText (string genericText, string actorName = null, bool? resetPrinter = null, bool? waitForInput = null)
        {
            var escapedText = genericText.Replace("\"", "\\\""); // Escape quotes in the printed text.
            var result = $"{ActionScriptLine.IdentifierLiteral}print text{ActionScriptLine.AssignLiteral}\"{escapedText}\"";
            if (!string.IsNullOrEmpty(actorName))
                result += $" actor{ActionScriptLine.AssignLiteral}{actorName}";
            if (resetPrinter.HasValue)
                result += $" reset{ActionScriptLine.AssignLiteral}{resetPrinter.Value}";
            if (waitForInput.HasValue)
                result += $" waitInput{ActionScriptLine.AssignLiteral}{waitForInput.Value}";
            return result;
        }
    }
}
