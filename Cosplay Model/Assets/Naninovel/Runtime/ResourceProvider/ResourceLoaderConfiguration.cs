// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable data used for <see cref="ResourceLoader"/> construction.
    /// </summary>
    [System.Serializable]
    public class ResourceLoaderConfiguration 
    {
        [Tooltip("Path prefix to add for each requested resource.")]
        public string PathPrefix = string.Empty;
        [Tooltip("Provider types to use, in order.")]
        public List<ResourceProviderType> ProviderTypes = new List<ResourceProviderType> { ResourceProviderType.Project, ResourceProviderType.Local };

        public ResourceLoader<TResource> CreateFor<TResource> (ResourceProviderManager providerManager) where TResource : Object
        {
            var providerList = providerManager.GetProviderList(ProviderTypes);
            return new ResourceLoader<TResource>(providerList, PathPrefix);
        }

        public override string ToString () => $"{PathPrefix}- ({string.Join(", ", ProviderTypes)})";
    }
}
