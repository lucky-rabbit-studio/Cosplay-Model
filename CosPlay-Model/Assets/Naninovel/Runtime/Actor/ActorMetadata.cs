// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IActor"/>.
    /// </summary>
    public abstract class ActorMetadata
    {
        public string Guid => guid;

        [Tooltip("Full type name of the actor implementation.")]
        public string Implementation = default;
        [Tooltip("Data describing how to load actor's resources.")]
        public ResourceLoaderConfiguration LoaderConfiguration = default;

        [HideInInspector]
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();
    }
}
