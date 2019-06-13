// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class ChoiceHandlersConfiguration : NovelActorManagerConfiguration
    {
        public const string DefaultChoiceHandlersPathPrefix = "ChoiceHandlers";

        [Tooltip("ID of the choice handler to use by default.")]
        public string DefaultHandlerId = "ButtonList";
        [Tooltip("Metadata to use by default when creating choice handler actors and custom metadata for the created actor ID doesn't exist.")]
        public ChoiceHandlerMetadata DefaultMetadata = new ChoiceHandlerMetadata();
        [Tooltip("Metadata to use when creating choice handler actors with specific IDs.")]
        public ChoiceHandlerMetadata.Map Metadata = new ChoiceHandlerMetadata.Map {
            ["ButtonList"] = new ChoiceHandlerMetadata {
                Implementation = typeof(UIChoiceHandler).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/ChoiceHandlers" },
            },
            ["ButtonArea"] = new ChoiceHandlerMetadata {
                Implementation = typeof(UIChoiceHandler).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/ChoiceHandlers" },
            }
        };
    }
}
