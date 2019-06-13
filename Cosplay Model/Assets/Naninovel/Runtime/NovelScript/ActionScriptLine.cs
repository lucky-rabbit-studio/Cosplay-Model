// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="NovelScript"/> line representing a single <see cref="NovelAction"/>.
    /// </summary>
    public class ActionScriptLine : NovelScriptLine
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
        /// In cases when inlined to a <see cref="GenericTextScriptLine"/>, represents index among other inlined action lines.
        /// Will return zero if not inlined.
        /// </summary>
        public int InlineIndex { get; } = 0;
        /// <summary>
        /// Name or tag of the action (string between <see cref="IdentifierLiteral"/> and white space). Case-insensitive.
        /// </summary>
        public string ActionName { get; }
        /// <summary>
        /// Parameters of the action represented by [paramater name] -> [value] map. Keys are case-insensitive.
        /// </summary>
        public LiteralMap<string> ActionParameters { get; }

        public ActionScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null)
            : this(scriptName, lineIndex, lineText, scriptDefines, 0) { } // For reflection to work properly.

        public ActionScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null, int inlineIndex = 0) 
            : base(scriptName, lineIndex, lineText, scriptDefines)
        {
            ActionName = ParseActionName(Text);
            Debug.Assert(!string.IsNullOrWhiteSpace(ActionName), ParseErrorMessage);

            ActionParameters = ParseActionParameters(Text, out var isError);
            Debug.Assert(!isError, ParseErrorMessage);

            InlineIndex = inlineIndex;
            Debug.Assert(InlineIndex >= 0, ParseErrorMessage);
        }

        private static string ParseActionName (string scriptLineText)
        {
            var actionName = scriptLineText.GetBetween(IdentifierLiteral, " ");
            if (string.IsNullOrEmpty(actionName))
                actionName = scriptLineText.GetAfter(IdentifierLiteral);
            return actionName;
        }

        private static LiteralMap<string> ParseActionParameters (string scriptLineText, out bool isError)
        {
            isError = false;
            var actionParameters = new LiteralMap<string>();

            var paramPairs = ExtractParamPairsFromScriptLine(scriptLineText);
            if (paramPairs is null) return actionParameters; // No params in the line.

            foreach (var paramPair in paramPairs)
            {
                var paramName = string.Empty;
                var paramValue = string.Empty;
                if (IsParamPairNameless(paramPair)) // Corner case for nameless params.
                {
                    if (actionParameters.ContainsKey(string.Empty))
                    {
                        Debug.LogError("There could be only one nameless parameter per action.");
                        isError = true;
                        return actionParameters;
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
                    return actionParameters;
                }

                // Trim quotes in case parameter value is wrapped in them.
                if (paramValue.WrappedIn("\""))
                    paramValue = paramValue.Substring(1, paramValue.Length - 2);

                // Restore escaped quotes.
                paramValue = paramValue.Replace("\\\"", "\"");

                actionParameters.Add(paramName, paramValue);
            }

            return actionParameters;
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
