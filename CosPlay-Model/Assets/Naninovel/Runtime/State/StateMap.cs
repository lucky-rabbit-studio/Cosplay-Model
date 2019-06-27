// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a map of objects serialized as JSON strings and indexed by their full type names.
    /// </summary>
    [Serializable]
    public class StateMap : SerializableMap<string, string>
    {
        /// <summary>
        /// Stores JSON representation of the provided object in the map using the object's full type name as the key.
        /// </summary>
        public void SerializeObject<T> (T obj) where T : class, new()
        {
            var fullTypeName = typeof(T).FullName;
            var stateJson = JsonUtility.ToJson(obj, Debug.isDebugBuild);
            this[fullTypeName] = stateJson;
        }

        /// <summary>
        /// Attempts to retrieve a JSON representation of an object with the provided type from the map and deserialize it. 
        /// Will return null when no objects of the type is contained in the map.
        /// </summary>
        public TState DeserializeObject<TState> () where TState : class, new()
        {
            var fullTypeName = typeof(TState).FullName;
            if (!ContainsKey(fullTypeName)) return null;
            var stateJson = this[fullTypeName];
            return JsonUtility.FromJson<TState>(stateJson);
        }
    }
}
