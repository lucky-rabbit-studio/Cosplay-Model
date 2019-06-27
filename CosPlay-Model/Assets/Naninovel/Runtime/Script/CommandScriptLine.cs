// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing a single <see cref="Command"/>.
    /// </summary>
    public class CommandScriptLine : ScriptLine
    {
        /// <summary>
        /// Literal used to identify this type of lines.
        /// </summary>
        public const string IdentifierLiteral = "@";
        /// <summary>
        /// Literal used to assign paramer values to their names.
        /// </summary>
        public const string AssignLiteral = ":";
        /// <summary>
        /// In cases when inlined to a <see cref="GenericTextScriptLine"/>, represents index among other inlined command lines.
        /// Will return zero if not inlined.
        /// </summary>
        public int InlineIndex { get; } = 0;
        /// <summary>
        /// Name or tag of the command (string between <see cref="IdentifierLiteral"/> and white space). Case-insensitive.
        /// </summary>
        public string CommandName { get; }
        /// <summary>
        /// Parameters of the command represented by [paramater name] -> [value] map. Keys are case-insensitive.
        /// </summary>
        public LiteralMap<string> CommandParameters { get; }

        public CommandScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null, bool ignoreErrors = false)
            : this(scriptName, lineIndex, lineText, scriptDefines, 0, ignoreErrors) { } // For reflection to work properly.

        public CommandScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null, int inlineIndex = 0, bool ignoreErrors = false) 
            : base(scriptName, lineIndex, lineText, scriptDefines, ignoreErrors)
        {
            CommandName = ParseCommandName(Text);
            Debug.Assert(!string.IsNullOrWhiteSpace(CommandName), ParseErrorMessage);

            CommandParameters = ParseCommandParameters(Text, out var isError, IgnoreParseErrors);
            Debug.Assert(!isError, ParseErrorMessage);

            InlineIndex = inlineIndex;
            Debug.Assert(InlineIndex >= 0, ParseErrorMessage);
        }

        private static string ParseCommandName (string scriptLineText)
        {
            var cmdName = scriptLineText.GetBetween(IdentifierLiteral, " ");
            if (string.IsNullOrEmpty(cmdName))
                cmdName = scriptLineText.GetAfter(IdentifierLiteral);
            return cmdName;
        }

        private static LiteralMap<string> ParseCommandParameters (string scriptLineText, out bool isError, bool ignoreErrors = false)
        {
            isError = false;
            var cmdParams = new LiteralMap<string>();

            var paramPairs = ExtractParamPairsFromScriptLine(scriptLineText);
            if (paramPairs is null) return cmdParams; // No params in the line.

            foreach (var paramPair in paramPairs)
            {
                var paramName = string.Empty;
                var paramValue = string.Empty;
                if (IsParamPairNameless(paramPair)) // Corner case for nameless params.
                {
                    if (cmdParams.ContainsKey(string.Empty))
                    {
                        if (ignoreErrors) continue;
                        Debug.LogError("There could be only one nameless parameter per command.");
                        isError = true;
                        return cmdParams;
                    }
                    paramValue = paramPair;
                }
                else
                {
                    paramName = paramPair.GetBefore(AssignLiteral);
                    paramValue = paramPair.GetAfterFirst(AssignLiteral);
                }

                if (paramName is null || paramValue is null)
                {
                    isError = true;
                    return cmdParams;
                }

                // Trim quotes in case parameter value is wrapped in them.
                if (paramValue.WrappedIn("\""))
                    paramValue = paramValue.Substring(1, paramValue.Length - 2);

                // Restore escaped quotes.
                paramValue = paramValue.Replace("\\\"", "\"");

                cmdParams.Add(paramName, paramValue);
            }

            return cmdParams;
        }

        /// <summary>
        /// Capture whitespace and tabs, but ignore regions inside (non-escaped) quotes.
        /// </summary>
        private static List<string> ExtractParamPairsFromScriptLine (string scriptLineText)
        {
            var paramStartIndex = scriptLineText.IndexOf(' ') + 1;
            if (paramStartIndex == 0) paramStartIndex = scriptLineText.IndexOf('\t') + 1; // Try tab.
            if (paramStartIndex == 0) return null; // No params in the line.

            var paramText = scriptLineText.Substring(paramStartIndex);
            var paramPairs = new List<string>();

            var captureStartIndex = -1;
            var isInsideQuotes = false;
            bool IsCapturing () => captureStartIndex >= 0;
            bool IsDelimeterChar (char c) => c == ' ' || c == '\t';
            bool IsQuotesAt (int index)
            {
                var c = paramText[index];
                if (c != '"') return false;
                if (index > 0 && paramText[index - 1] == '\\') return false;
                return true;
            }
            void StartCaptureAt (int index) => captureStartIndex = index;
            void FinishCaptureAt (int index)
            {
                var paramPair = paramText.Substring(captureStartIndex, index - captureStartIndex + 1);
                paramPairs.Add(paramPair);
                captureStartIndex = -1;
            }

            for (int i = 0; i < paramText.Length; i++)
            {
                var c = paramText[i];

                if (!IsCapturing() && IsDelimeterChar(c)) continue;
                if (!IsCapturing()) StartCaptureAt(i);

                if (IsQuotesAt(i))
                    isInsideQuotes = !isInsideQuotes;
                if (isInsideQuotes) continue;

                if (IsDelimeterChar(c))
                {
                    FinishCaptureAt(i - 1);
                    continue;
                }

                if (i == (paramText.Length - 1))
                    FinishCaptureAt(i);
            }

            return paramPairs;
        }

        /// <summary>
        /// The string doesn't contain assign literal, or it's within (non-escaped) quotes.
        /// </summary>
        private static bool IsParamPairNameless (string paramPair)
        {
            if (!paramPair.Contains(AssignLiteral)) return true;

            var assignChar = AssignLiteral[0];
            var isInsideQuotes = false;
            bool IsQuotesAt (int index)
            {
                var c = paramPair[index];
                if (c != '"') return false;
                if (index > 0 && paramPair[index - 1] == '\\') return false;
                return true;
            }

            for (int i = 0; i < paramPair.Length; i++)
            {
                if (IsQuotesAt(i))
                    isInsideQuotes = !isInsideQuotes;
                if (isInsideQuotes) continue;

                if (paramPair[i] == assignChar) return false;
            }

            return true;
        }
    }
}
