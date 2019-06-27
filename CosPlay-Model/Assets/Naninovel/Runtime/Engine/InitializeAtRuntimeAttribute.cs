// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// When applied to a <see cref="IEngineService"/> implementation,
    /// adds the service to the initialization list of <see cref="RuntimeInitializer"/>.
    /// </summary>
    /// <remarks>
    /// Requires the implementation to have either a default ctor, or a ctor with the following parameters:
    /// <see cref="IEngineService"/> with this attribute applied or <see cref="IEngineBehaviour"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class InitializeAtRuntimeAttribute : Attribute
    {
        public int InitializationPriority { get; }

        public InitializeAtRuntimeAttribute (int initializationPriority = 0)
        {
            InitializationPriority = initializationPriority;
        }
    }
}
