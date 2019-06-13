﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="ITextPrinterActor"/>.
    /// </summary>
    [System.Serializable]
    public class TextPrinterMetadata : NovelActorMetadata
    {
        [System.Serializable]
        public class Map : NovelActorMetadataMap<TextPrinterMetadata> { }

        public TextPrinterMetadata ()
        {
            Implementation = typeof(UITextPrinter).FullName;
            LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = TextPrintersConfiguration.DefaultTextPrintersPathPrefix };
        }
    }
}