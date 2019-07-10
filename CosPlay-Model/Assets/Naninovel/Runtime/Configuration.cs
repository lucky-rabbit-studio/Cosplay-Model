// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Base class for the project-specific configuration assets used to initialize and configure engine services and other systems.
    /// Serialized configuration assets are generated automatically under the engine's data folder and can be edited via Unity project settings menu.
    /// </summary>
    public abstract class Configuration : ScriptableObject
    {
        /// <summary>
        /// Path relative to a `Resources` folder under which the generated configuration assets are stored.
        /// </summary>
        public const string ResourcesPath = "Naninovel/Configuration";

        /// <summary>
        /// Attempts to load a configuration asset of the specified type from the project resources.
        /// When the requested configuration asset doesn't exist, will create a default one instead.
        /// </summary>
        /// <typeparam name="TConfig">Type of the requested configuration asset.</typeparam>
        /// <returns>Deserialized version of the requested configuration asset (when exists) or a new default one.</returns>
        public static TConfig LoadOrDefault<TConfig> () where TConfig : Configuration, new() => LoadOrDefault(typeof(TConfig)) as TConfig;

        /// <summary>
        /// Same as <see cref="LoadOrDefault{TConfig}"/> but without the type checking.
        /// </summary>
        public static Configuration LoadOrDefault (Type type)
        {
            var resourcePath = $"{ResourcesPath}/{type.Name}";
            var config = Resources.Load(resourcePath, type) as Configuration;

            if (!ObjectUtils.IsValid(config))
                return CreateInstance(type) as Configuration;

            return config;
        }
    }
}
