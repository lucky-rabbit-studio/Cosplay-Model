// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Manages variables set by user in the novel scripts.
    /// </summary>
    [InitializeAtRuntime]
    public class CustomVariableManager : IStatefulService<GameStateMap>, IStatefulService<GlobalStateMap>
    {
        [System.Serializable]
        private class VariableMap : SerializableLiteralStringMap { }

        /// <summary>
        /// Invoked when a custom variable is created or its value changed.
        /// </summary>
        public event Action<CustomVariableUpdatedArgs> OnVariableUpdated;

        /// <summary>
        /// Custom variable name prefix (case-insensitive) used to indicate a global variable.
        /// </summary>
        public const string GlobalPrefix = "G_";

        private VariableMap localVariableMap;
        private VariableMap globalVariableMap;

        public CustomVariableManager ()
        {
            localVariableMap = new VariableMap();
            globalVariableMap = new VariableMap();
        }

        public Task InitializeServiceAsync () => Task.CompletedTask;

        public void ResetService () { }

        public void DestroyService () { }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            stateMap.SerializeObject(localVariableMap);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            localVariableMap = stateMap.DeserializeObject<VariableMap>() ?? new VariableMap();
            return Task.CompletedTask;
        }

        public Task SaveServiceStateAsync (GlobalStateMap stateMap)
        {
            stateMap.SerializeObject(globalVariableMap);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GlobalStateMap stateMap)
        {
            globalVariableMap = stateMap.DeserializeObject<VariableMap>() ?? new VariableMap();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks whether a custom variable with the provided name is global.
        /// </summary>
        public bool IsGlobalVariable (string name) => name.StartsWithFast(GlobalPrefix.ToLowerInvariant());

        /// <summary>
        /// Checks whether a variable with the provided name exists.
        /// </summary>
        public bool VariableExists (string name) => IsGlobalVariable(name) ? globalVariableMap.ContainsKey(name) : localVariableMap.ContainsKey(name);

        /// <summary>
        /// Attempts to retrive value of a variable with the provided name. Variable names are case-insensitive. 
        /// When no variables of the provided name are found will return null.
        /// </summary>
        public string GetVariableValue (string name)
        {
            if (!VariableExists(name)) return null;
            return IsGlobalVariable(name) ? globalVariableMap[name] : localVariableMap[name];
        }

        /// <summary>
        /// Sets value of a variable with the provided name. Variable names are case-insensitive. 
        /// When no variables of the provided name are found, will add a new one and assign the value.
        /// In case the name is starting with <see cref="GlobalPrefix"/>, the variable will be added to the global scope.
        /// </summary>
        public void SetVariableValue (string name, string value)
        {
            var isGlobal = IsGlobalVariable(name);
            var initialValue = default(string);

            if (isGlobal)
            {
                globalVariableMap.TryGetValue(name, out initialValue);
                globalVariableMap[name] = value;
            }
            else
            {
                localVariableMap.TryGetValue(name, out initialValue);
                localVariableMap[name] = value;
            }

            if (initialValue != value)
                OnVariableUpdated?.Invoke(new CustomVariableUpdatedArgs(name, value, initialValue));
        }

        /// <summary>
        /// Purges all the custom local state variables.
        /// </summary>
        public void ResetLocalVariables ()
        {
            localVariableMap?.Clear();
        }

        /// <summary>
        /// Purges all the custom global state variables.
        /// </summary>
        public void ResetGlobalVariables ()
        {
            globalVariableMap?.Clear();
        }

        /// <summary>
        /// Attempts to parse the provided value string into float (the string should contain a dot), integer and then boolean.
        /// When parsing fails will return the initial string.
        /// </summary>
        public static object ParseVariableValue (string value)
        {
            if (value.Contains(".") && ParseUtils.TryInvariantFloat(value, out var floatValue)) return floatValue;
            else if (ParseUtils.TryInvariantInt(value, out var intValue)) return intValue;
            else if (bool.TryParse(value, out var boolValue)) return boolValue;
            else return value;
        }
    }
}
