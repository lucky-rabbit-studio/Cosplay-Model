// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IChoiceHandlerActor"/>.
    /// </summary>
    [System.Serializable]
    public class ChoiceHandlerMetadata : ActorMetadata
    {
        [System.Serializable]
        public class Map : ActorMetadataMap<ChoiceHandlerMetadata> { }

        public ChoiceHandlerMetadata ()
        {
            Implementation = typeof(UIChoiceHandler).FullName;
            LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = ChoiceHandlersConfiguration.DefaultChoiceHandlersPathPrefix };
        }
    }
}
