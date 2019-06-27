// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing text to print.
    /// Could contain actor name at the start of the line followed by a column,
    /// and multiple inlined <see cref="CommandScriptLine"/> enclosed in square brackets.
    /// </summary>
    public class GenericTextScriptLine : ScriptLine
    {
        /// <summary>
        /// Literal used to declare actor name before the text to print.
        /// </summary>
        public const string ActorNameLiteral = ": ";
        /// <summary>
        /// A list of <see cref="CommandScriptLine"/> inlined in this line.
        /// </summary>
        public List<CommandScriptLine> InlinedCommandLines { get; }

        public GenericTextScriptLine (string scriptName, int lineNumber, string lineText, LiteralMap<string> scriptDefines = null, bool ignoreErrors = false) 
            : base(scriptName, lineNumber, lineText, scriptDefines, ignoreErrors)
        {
            InlinedCommandLines = ExtractInlinedCommandLines(Text, scriptDefines);
        }

        public GenericTextScriptLine (string scriptName, int lineNumber, List<CommandScriptLine> inlinedCommandLines, bool ignoreErrors = false) 
            : base(scriptName, lineNumber, string.Empty, ignoreParseErrors: ignoreErrors)
        {
            InlinedCommandLines = inlinedCommandLines;
        }

        protected override string ReplaceDefines (string lineText, LiteralMap<string> defines)
        {
            foreach (var define in defines) // Actor names in generic text lines doesn't require replace literal to be replaced.
                if (lineText.StartsWithFast($"{define.Key}: ")) { lineText = lineText.Replace($"{define.Key}: ", $"{define.Value}: "); break; }

            return base.ReplaceDefines(lineText, defines);
        }

        private List<CommandScriptLine> ExtractInlinedCommandLines (string lineText, LiteralMap<string> scriptDefines = null)
        {
            // When actor name is present at the start of the line: extract it and cut from the line.
            var actorName = lineText.GetBefore(ActorNameLiteral);
            if (!string.IsNullOrEmpty(actorName) && !actorName.Any(char.IsWhiteSpace) && !actorName.StartsWithFast("\""))
                lineText = lineText.GetAfterFirst(ActorNameLiteral);
            else actorName = null;

            // Collect all inlined command strings (text inside square brackets).
            var inlinedCommandMatches = Regex.Matches(lineText, "\\[.*?\\]").Cast<Match>().ToList();

            // In case no inlined commands found, just add a single @print command line.
            if (inlinedCommandMatches.Count == 0)
            {
                var printLineText = TransformGenericToPrintText(lineText, actorName);
                var printLine = new CommandScriptLine(ScriptName, LineIndex, printLineText, scriptDefines, ignoreErrors: IgnoreParseErrors);
                return new List<CommandScriptLine> { printLine };
            }

            var result = new List<CommandScriptLine>();
            var printedTextBefore = false;
            for (int i = 0; i < inlinedCommandMatches.Count; i++)
            {
                // Check if we need to print any text before the current inlined command.
                var precedingGenericText = StringUtils.TrySubset(lineText,
                    i > 0 ? inlinedCommandMatches[i - 1].GetEndIndex() + 1 : 0,
                    inlinedCommandMatches[i].Index - 1);
                if (!string.IsNullOrEmpty(precedingGenericText))
                {
                    var printLineText = TransformGenericToPrintText(precedingGenericText, actorName, printedTextBefore ? (bool?)false : null, false);
                    var printLine = new CommandScriptLine(ScriptName, LineIndex, printLineText, scriptDefines, result.Count, IgnoreParseErrors);
                    result.Add(printLine);
                    printedTextBefore = true;
                }

                // Extract inlined command script line.
                var commandLineText = CommandScriptLine.IdentifierLiteral + inlinedCommandMatches[i].ToString().GetBetween("[", "]").TrimFull();
                var commandLine = new CommandScriptLine(ScriptName, LineIndex, commandLineText, scriptDefines, result.Count, IgnoreParseErrors);
                result.Add(commandLine);
            }

            // Check if we need to print any text after the last inlined command.
            var lastGenericText = StringUtils.TrySubset(lineText,
                     inlinedCommandMatches.Last().GetEndIndex() + 1,
                     lineText.Length - 1);
            if (!string.IsNullOrEmpty(lastGenericText))
            {
                var printLineText = TransformGenericToPrintText(lastGenericText, actorName, printedTextBefore ? (bool?)false : null, false);
                var printLine = new CommandScriptLine(ScriptName, LineIndex, printLineText, scriptDefines, result.Count, IgnoreParseErrors);
                result.Add(printLine);
            }

            // Add wait input command at the end.
            var waitCommandLineText = CommandScriptLine.IdentifierLiteral + typeof(WaitForInput).Name;
            var waitCommandLine = new CommandScriptLine(ScriptName, LineIndex, waitCommandLineText, scriptDefines, result.Count, IgnoreParseErrors);
            result.Add(waitCommandLine);

            return result;
        }

        /// <summary>
        /// Transforms a generic text string to print command line text, which can be used 
        /// to create an <see cref="CommandScriptLine"/> for <see cref="PrintText"/> command.
        /// </summary>
        private static string TransformGenericToPrintText (string genericText, string actorName = null, bool? resetPrinter = null, bool? waitForInput = null)
        {
            var escapedText = genericText.Replace("\"", "\\\""); // Escape quotes in the printed text.
            var result = $"{CommandScriptLine.IdentifierLiteral}print text{CommandScriptLine.AssignLiteral}\"{escapedText}\"";
            if (!string.IsNullOrEmpty(actorName))
                result += $" actor{CommandScriptLine.AssignLiteral}{actorName}";
            if (resetPrinter.HasValue)
                result += $" reset{CommandScriptLine.AssignLiteral}{resetPrinter.Value}";
            if (waitForInput.HasValue)
                result += $" waitInput{CommandScriptLine.AssignLiteral}{waitForInput.Value}";
            return result;
        }
    }
}
