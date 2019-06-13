// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Represents a <see cref="NovelScript"/> action.
    /// </summary>
    public abstract class NovelAction
    {
        /// <summary>
        /// Implementing <see cref="NovelAction"/> will be included in localization scripts.
        /// </summary>
        public interface ILocalizable { }

        /// <summary>
        /// Implementing <see cref="NovelAction"/> is able to asynchronously preload/unload required resources.
        /// </summary>
        public interface IPreloadable
        {
            Task PreloadResourcesAsync ();
            Task UnloadResourcesAsync ();
        }

        /// <summary>
        /// Assigns an alias name for <see cref="NovelAction"/>.
        /// Aliases can be used instead of the action IDs (type names) to reference actions in novel script.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
        protected sealed class ActionAliasAttribute : Attribute
        {
            public string Alias { get; }

            public ActionAliasAttribute (string alias)
            {
                Alias = alias;
            }
        }

        /// <summary>
        /// Registers the field as <see cref="NovelAction"/> parameter.
        /// </summary>
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
        protected sealed class ActionParameterAttribute : Attribute
        {
            /// <summary>
            /// Used to reference the parameter in novel action script.
            /// Field name will be used if not provided. 
            /// </summary>
            public string Alias { get; }
            /// <summary>
            /// Whether the parameter can be ommited when declaring novel action in the script.
            /// </summary>
            public bool Optional { get; }

            /// <param name="alias">Alias name of the parameter. Can serve as a shortcut instead of the parameter ID (property name).</param>
            /// <param name="optional">Whether the parameter can be omitted.</param>
            public ActionParameterAttribute (string alias = null, bool optional = false)
            {
                Alias = alias;
                Optional = optional;
            }
        }

        /// <summary>
        /// Use this alias to specify a nameless action parameter.
        /// </summary>
        public const string NamelessParameterAlias = "";

        /// <summary>
        /// Whether this action was created from a <see cref="ActionScriptLine"/>.
        /// </summary>
        public bool IsFromScriptLine => !string.IsNullOrEmpty(ScriptName);
        /// <summary>
        /// When <see cref="IsFromScriptLine"/>, returns <see cref="NovelScript.Name"/>.
        /// </summary>
        public string ScriptName { get; private set; }
        /// <summary>
        /// When <see cref="IsFromScriptLine"/>, returns <see cref="NovelScriptLine.LineIndex"/>.
        /// </summary>
        public int LineIndex { get; private set; }
        /// <summary>
        /// When <see cref="IsFromScriptLine"/>, returns <see cref="NovelScriptLine.LineNumber"/>.
        /// </summary>
        public int LineNumber { get; private set; }
        /// <summary>
        /// When <see cref="IsFromScriptLine"/>, returns <see cref="ActionScriptLine.InlineIndex"/>.
        /// </summary>
        public int InlineIndex { get; private set; }
        /// <summary>
        /// Whether this action should be executed, as per <see cref="ConditionalExpression"/>.
        /// </summary>
        public bool ShouldExecute => string.IsNullOrEmpty(ConditionalExpression) || ExpressionEvaluator.Evaluate<bool>(ConditionalExpression);

        /// <summary>
        /// Whether the script player should wait for the async action execution before playing next action.
        /// </summary>
        [ActionParameter(optional: true)]
        public bool Wait { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }
        /// <summary>
        /// Determines for how long (in seconds) action should execute. Derived actions could (or could not) use this parameter.
        /// </summary>
        [ActionParameter("time", true)]
        public float Duration { get => GetDynamicParameter(.35f); set => SetDynamicParameter(value); }
        /// <summary>
        /// A boolean [script expression](/guide/script-expressions.md), controlling whether this action should execute.
        /// </summary>
        [ActionParameter("if", true)]
        public string ConditionalExpression { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        // Capture injected script expressions enclosed in (not-escaped) `{}`.
        private static readonly Regex captureVarsRegex = new Regex(@"(?<!\\)\{(.*?)(?<!\\)\}");

        /// <summary>
        /// Used to cache all the novel action types to speedup de-/serialization; typename->type.
        /// When action tag is defined, it will be used instead of typename. Keys are case-insensitive.
        /// </summary>
        private static LiteralMap<Type> cachedActionTypes;

        /// <summary>
        /// Used to store funcs that evaluates dynamic novel action parameters values: [param name] -> [get value func].
        /// Required to handle the cases when a script expression in injected to parameter value in novel scripts.
        /// </summary>
        private Dictionary<string, Func<object>> dynamicParameters = new Dictionary<string, Func<object>>(StringComparer.Ordinal);

        static NovelAction ()
        {
            if (cachedActionTypes is null)
                CacheActionTypes();
        }

        /// <summary>
        /// Creates new instance from serialized action text in form of a <see cref="ActionScriptLine"/>.
        /// </summary>
        public static NovelAction FromScriptLine (ActionScriptLine scriptLine)
        {
            var actionType = FindActionType(scriptLine.ActionName);
            if (actionType is null)
            {
                Debug.LogError($"Script `{scriptLine.ScriptName}` at line #{scriptLine.LineNumber}: action `{scriptLine.ActionName}` not found in project's novel action types.");
                return null;
            }

            var action = Activator.CreateInstance(actionType) as NovelAction;

            var paramaterFieldInfos = actionType.GetProperties()
                .Where(property => property.IsDefined(typeof(ActionParameterAttribute), false));
            foreach (var paramFieldInfo in paramaterFieldInfos)
            {
                var paramAttribute = paramFieldInfo.GetCustomAttributes(typeof(ActionParameterAttribute), false)
                    .FirstOrDefault() as ActionParameterAttribute;
                Debug.Assert(paramAttribute != null);

                var paramName = paramAttribute.Alias != null && scriptLine.ActionParameters.ContainsKey(paramAttribute.Alias) ? paramAttribute.Alias : paramFieldInfo.Name;
                if (!scriptLine.ActionParameters.ContainsKey(paramName))
                {
                    if (!paramAttribute.Optional)
                        Debug.LogError($"Script `{scriptLine.ScriptName}` at line #{scriptLine.LineNumber}: action `{actionType.Name}` is missing `{paramName}` parameter.");
                    continue;
                }

                var paramValueString = scriptLine.ActionParameters[paramName];
                // Check for injected script expressions and use late binding when found.
                var matches = captureVarsRegex.Matches(paramValueString);
                // Un-escape the `{` and `}` literals.
                paramValueString = paramValueString.Replace("\\{", "{").Replace("\\}", "}");
                if (matches.Count > 0) BindDynamicParameter(action, paramFieldInfo.Name, paramValueString, paramFieldInfo.PropertyType, matches);
                else // ...otherwise, parse and set the parameter value.
                {
                    var paramValue = ParseParameterValue(paramValueString, paramFieldInfo.PropertyType);
                    paramFieldInfo.SetValue(action, paramValue);
                }
            }

            action.OnDeserializedFromScriptLine(scriptLine);

            return action;
        }

        /// <summary>
        /// Checks whether provided script line contains any <see cref="ILocalizable"/> actions.
        /// </summary>
        public static bool IsLineLocalizable (NovelScriptLine scriptLine)
        {
            switch (scriptLine)
            {
                case GenericTextScriptLine genericLine:
                    return true;
                case ActionScriptLine actionLine:
                    var type = FindActionType(actionLine.ActionName);
                    if (type is null) return false;
                    return typeof(ILocalizable).IsAssignableFrom(type);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Executes the action asynchronously.
        /// </summary>
        public abstract Task ExecuteAsync ();

        /// <summary>
        /// Reverts the effect of <see cref="ExecuteAsync"/> invocation. Tempus edax rerum.
        /// </summary>
        public abstract Task UndoAsync ();

        /// <summary>
        /// Invoked when the instance is created from a <see cref="ActionScriptLine"/> and 
        /// all the <see cref="ActionParameterAttribute"/>-fields are populated.
        /// </summary>
        protected virtual void OnDeserializedFromScriptLine (ActionScriptLine scriptLine)
        {
            ScriptName = scriptLine.ScriptName;
            LineIndex = scriptLine.LineIndex;
            LineNumber = scriptLine.LineNumber;
            InlineIndex = scriptLine.InlineIndex;
        }

        /// <summary>
        /// Attempts to evaluate value of the parameter with the provided name and type; returns provided default value when 
        /// the specified parameter doesn't have a dynamic binding set.
        /// </summary>
        /// <remarks>With dynamic it would look much cleaner, but AOT platforms doesn't seem to support it.</remarks>
        protected TParameter GetDynamicParameter<TParameter> (TParameter defaultValue, [CallerMemberName] string parameterName = null)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(parameterName), "Failed to get dynamic parameter: invalid parameter name.");

            var keyFound = dynamicParameters.TryGetValue(parameterName, out var getter);
            if (!keyFound || getter is null) return defaultValue;

            var value = getter.Invoke();
            return value is null ? default : (TParameter)value;
        }

        /// <summary>
        /// Parameter property with the provided name will always return the provided value.
        /// Used to set a dynamic parameter to a non-dynamic value (in cases when late binding is not required).
        /// </summary>
        protected void SetDynamicParameter (object parameterValue, [CallerMemberName] string parameterName = null)
        {
            #pragma warning disable IDE0039 // Use local function.
            Func<object> GetValue = () => parameterValue;
            #pragma warning restore IDE0039 // Use local function.
            SetDynamicParameter(GetValue, parameterName);
        }

        /// <summary>
        /// Provided delegate will be used to evalutate value of the action parameter with the provided name.
        /// Used for late binding dynamic parameter values.
        /// </summary>
        protected void SetDynamicParameter (Func<object> parameterValue, [CallerMemberName] string parameterName = null)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(parameterName), "Failed to set dynamic parameter: invalid parameter name.");
            dynamicParameters[parameterName] = parameterValue;
        }

        private static Type FindActionType (string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            // First, try to resolve by key.
            cachedActionTypes.TryGetValue(typeName, out Type result);
            // If not found, look by typename (in case typename was used for an action with defined tag).
            if (result is null)
                result = cachedActionTypes.Values.FirstOrDefault(actionType => actionType.Name.EqualsFastIgnoreCase(typeName));
            return result;
        }

        private static void CacheActionTypes ()
        {
            cachedActionTypes = new LiteralMap<Type>();
            var actionTypes = ReflectionUtils.ExportedDomainTypes
                .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(NovelAction)));
            foreach (var actionType in actionTypes)
            {
                var actionKey = actionType.GetCustomAttributes(typeof(ActionAliasAttribute), false)
                    .FirstOrDefault() is ActionAliasAttribute tagAttribute ? tagAttribute.Alias : actionType.Name;
                cachedActionTypes.Add(actionKey, actionType);
            }
        }

        private static void BindDynamicParameter (NovelAction action, string paramName, string paramValueString, Type paramType, MatchCollection regexMatches)
        {
            action.SetDynamicParameter(() => {
                var valueString = paramValueString;
                foreach (Match match in regexMatches)
                {
                    var expression = match.Value.GetBetween("{", "}");
                    var varValue = ExpressionEvaluator.Evaluate<string>(expression, Debug.LogError);
                    valueString = valueString.Replace(match.Value, varValue);
                }
                return ParseParameterValue(valueString, paramType);
            }, paramName);
        }

        private static object ParseParameterValue (string paramValueString, Type paramType)
        {
            if (paramValueString is null) return null;

            var nullableType = Nullable.GetUnderlyingType(paramType);
            if (nullableType != null)
                paramType = nullableType;

            // Named value (pair of string and a generic value), eg `Kohaku.25`.
            if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Named<>))
            {
                var name = paramValueString.Contains(".") ? paramValueString.GetBefore(".") : paramValueString;
                var valueType = paramType.GetGenericArguments()[0];
                var valueString = paramValueString.GetAfterFirst(".");
                var value = ParseParameterValue(valueString, valueType);
                var namedValueType = typeof(Named<>).MakeGenericType(valueType);
                return Activator.CreateInstance(namedValueType, name, value);
            }

            // Array of values, eg `1,,2,3`.
            if (paramType.IsArray)
            {
                var strValues = paramValueString.Split(',');
                for (int i = 0; i < strValues.Length; i++) // Replace empty strings with nulls to represent default params.
                    if (string.IsNullOrEmpty(strValues[i])) strValues[i] = null;
                var objValues = strValues.Select(s => ParseParameterValue(s, paramType.GetElementType())).ToArray();
                var array = Array.CreateInstance(paramType.GetElementType(), objValues.Length);
                for (int i = 0; i < objValues.Length; i++)
                    array.SetValue(objValues[i], i);
                return array;
            }

            // Simple value.
            try { return Convert.ChangeType(paramValueString, paramType, System.Globalization.CultureInfo.InvariantCulture); }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to parse parameter value `{paramValueString}` of type `{paramType}`. Default type value will be used. Error message: {e.Message}");
                return paramType.IsValueType ? Activator.CreateInstance(paramType) : null;
            }
        }
    } 
}
