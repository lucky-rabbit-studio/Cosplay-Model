// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IBackgroundActor"/>.
    /// </summary>
    [System.Serializable]
    public class BackgroundMetadata : OrthoActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<BackgroundMetadata> { }

        public BackgroundMetadata ()
        {
            Implementation = typeof(SpriteBackground).FullName;
            LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = BackgroundsConfiguration.DefaultBackgroundsPathPrefix };
            Pivot = new Vector2(.5f, .5f);
        }
    }
}
