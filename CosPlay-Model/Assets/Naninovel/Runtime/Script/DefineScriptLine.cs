// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing a define expression.
    /// </summary>
    public class DefineScriptLine : ScriptLine
    {
        /// <summary>
        /// Literal used to identify this type of lines.
        /// </summary>
        public const string IdentifierLiteral = ">";
        /// <summary>
        /// Key of the define expression (string between the `>` symbol and white space).
        /// </summary>
        public string DefineKey { get; }
        /// <summary>
        /// Value of the define expression (trimmed string after the <see cref="DefineKey"/>).
        /// </summary>
        public string DefineValue { get; }

        public DefineScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null, bool ignoreErrors = false) 
            : base(scriptName, lineIndex, lineText, scriptDefines, ignoreErrors)
        {
            DefineKey = ParseDefineKey(Text);
            DefineValue = ParseDefineValue(Text, DefineKey);

            if (scriptDefines != null)
            {
                if (!IgnoreParseErrors)
                    Debug.Assert(!string.IsNullOrEmpty(DefineKey) && !string.IsNullOrEmpty(DefineValue), ParseErrorMessage);
                //if (scriptDefines.ContainsKey(DefineKey)) Debug.LogWarning($"Multiple assigns to `{DefineKey}` define key detected in '{ScriptName}' script at line #{LineNumber}. Define value will be overwritten.");
                scriptDefines[DefineKey] = DefineValue;
            }
        }

        // Don't allow to replace the define expressions themselves.
        protected override string ReplaceDefines (string lineText, LiteralMap<string> defines) => lineText;

        private static string ParseDefineKey (string lineText) => lineText?.GetBetween(IdentifierLiteral, " ");

        private static string ParseDefineValue (string lineText, string defineKey) => lineText?.GetAfterFirst(defineKey)?.TrimFull();
    }
}
