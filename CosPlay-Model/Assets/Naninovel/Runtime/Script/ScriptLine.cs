// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Represents a single line in a <see cref="Script"/>.
    /// </summary>
    public abstract class ScriptLine
    {
        /// <summary>
        /// Represents persistent hash code of the script line content in hex format.
        /// </summary>
        public string ContentHash => CryptoUtils.PersistentHexCode(Text.TrimFull());
        /// <summary>
        /// Name of the naninovel script to which the line belongs.
        /// </summary>
        public readonly string ScriptName;
        /// <summary>
        /// Index of the line in naninovel script.
        /// </summary>
        public readonly int LineIndex;
        /// <summary>
        /// Number of the line in naninovel script (index + 1).
        /// </summary>
        public int LineNumber => LineIndex + 1;
        /// <summary>
        /// Text representation of the line.
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// A generic log message used when parsing fails.
        /// </summary>
        protected string ParseErrorMessage => $"{GetType().Name}: Error parsing `{ScriptName}` script at line #{LineNumber}.";
        /// <summary>
        /// Whether parsing errors should be silently ignored.
        /// </summary>
        protected readonly bool IgnoreParseErrors;

        public ScriptLine (string scriptName, int lineIndex, string lineText, LiteralMap<string> scriptDefines = null, bool ignoreParseErrors = false)
        {
            ScriptName = scriptName;
            LineIndex = lineIndex;
            Text = scriptDefines != null ? ReplaceDefines(lineText, scriptDefines) : lineText;
            IgnoreParseErrors = ignoreParseErrors;
        }

        /// <summary>
        /// Replaces all occurences of replace defined expressions in the provided string with the provided values.
        /// </summary>
        /// <remarks>
        /// Defines collection should be passed by reference to all the line constructors by line order; 
        /// <see cref="DefineScriptLine"/> will add values to the collection, while the subsequent lines 
        /// will use this method to replace the expressions in their <see cref="Text"/>.
        /// </remarks>
        protected virtual string ReplaceDefines (string lineText, LiteralMap<string> defines)
        {
            foreach (var define in defines)
                lineText = lineText.Replace($"{{{define.Key}}}", define.Value);

            return lineText;
        }
    } 
}
