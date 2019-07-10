// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Collection of utils to work with <see cref="Script"/> localization.
    /// </summary>
    /// <remarks>
    /// Localization script format:
    /// # {source line content hash in hex}
    /// ; Commentary with the original line's path and content.
    /// @action1
    /// @action2
    /// ...
    /// </remarks>
    public static class ScriptLocalization
    {
        /// <summary>
        /// Replaces lines of the <paramref name="sourceScript"/> with the lines from <paramref name="localizationScript"/>, 
        /// that have equal <see cref="ScriptLine.ContentHash"/>. 
        /// </summary>
        public static void LocalizeScript (Script sourceScript, Script localizationScript)
        {
            if (localizationScript.LabelLines.Count() == 0)
                return; // No translation terms in the localization script.

            var localizationTerms = GenerateLocalizationTerms(localizationScript);

            for (int i = 0; i < sourceScript.Lines.Count; i++)
            {
                var sourceLine = sourceScript.Lines[i];
                var contentHash = sourceLine.ContentHash;
                if (!localizationTerms.ContainsKey(contentHash)) continue;

                var localizedActionsText = localizationTerms[contentHash].SelectMany(t => ExtractActionsFromLine(t)); 
                var localizedActions = localizedActionsText.Select((lineText, inlineIndex) => 
                    new CommandScriptLine(sourceLine.ScriptName, sourceLine.LineIndex, lineText, null, inlineIndex)).ToList();
                // Single generic line containing all the localized lines added for the source line.
                sourceScript.Lines[i] = new GenericTextScriptLine(sourceLine.ScriptName, sourceLine.LineIndex, localizedActions);
            }
        }

        /// <summary>
        /// Groups lines from the localization script to [content hash] -> [line text] map.
        /// </summary>
        public static Dictionary<string, List<string>> GenerateLocalizationTerms (Script localizationScript)
        {
            var hashToLines = new Dictionary<string, List<string>>();
            string lineHash = null;
            var lines = new List<string>();
            foreach (var line in localizationScript.Lines)
            {
                if (line is CommentScriptLine) continue;
                if (line is LabelScriptLine)
                {
                    var commentLine = line as LabelScriptLine;

                    if (lineHash is null) { lineHash = commentLine.LabelText; continue; } // First term in the script.
                    if (lines.Count == 0) { lineHash = commentLine.LabelText; continue; } // Current term contained no command lines, skipping.

                    // Next term encountered; filling command lines for the previous one.
                    if (!hashToLines.ContainsKey(lineHash))
                        hashToLines.Add(lineHash, new List<string>(lines));
                    lineHash = commentLine.LabelText;
                    lines.Clear();
                    continue;
                }
                if (lineHash is null) // Encountered command without previous label; warn and continue.
                {
                    Debug.LogWarning($"Possibly malformed localization script encountered: script `{localizationScript.Name}` at line #{line.LineNumber}.");
                    continue;
                }
                lines.Add(line.Text);
            }
            if (lineHash != null && lines.Count > 0 && !hashToLines.ContainsKey(lineHash))
                hashToLines.Add(lineHash, lines); // Handle the last term.

            return hashToLines;
        }

        /// <summary>
        /// Attempts to extract all command lines text from provided line text.
        /// In cases when the line is <see cref="GenericTextScriptLine"/>, all the inlined command's text will be added.
        /// </summary>
        private static List<string> ExtractActionsFromLine (string lineText)
        {
            var result = new List<string>();
            var lineType = Script.ResolveLineType(lineText);
            if (lineType == typeof(CommandScriptLine)) result.Add(lineText);
            if (lineType == typeof(GenericTextScriptLine)) result.AddRange(new GenericTextScriptLine(null, 0, lineText).InlinedCommandLines.Select(a => a.Text));
            return result;
        }
    }
}
