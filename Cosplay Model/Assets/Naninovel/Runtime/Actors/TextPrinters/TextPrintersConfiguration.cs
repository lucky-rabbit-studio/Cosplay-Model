// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public class TextPrintersConfiguration : NovelActorManagerConfiguration
    {
        public const string DefaultTextPrintersPathPrefix = "TextPrinters";

        [Tooltip("ID of the text printer to use by default.")]
        public string DefaulPrinterId = "Dialogue";
        [Tooltip("Max typing delay. Determines print speed interval."), Range(.01f, 1.0f)]
        public float MaxPrintDelay = .06f;
        [Tooltip("Metadata to use by default when creating text printer actors and custom metadata for the created actor ID doesn't exist.")]
        public TextPrinterMetadata DefaultMetadata = new TextPrinterMetadata();
        [Tooltip("Metadata to use when creating text printer actors with specific IDs.")]
        public TextPrinterMetadata.Map Metadata = new TextPrinterMetadata.Map {
            ["Dialogue"] = new TextPrinterMetadata {
                Implementation = typeof(UITextPrinter).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/TextPrinters" },
            },
            ["Fullscreen"] = new TextPrinterMetadata {
                Implementation = typeof(UITextPrinter).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/TextPrinters" },
            },
            ["Wide"] = new TextPrinterMetadata {
                Implementation = typeof(UITextPrinter).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/TextPrinters" },
            },
            ["Chat"] = new TextPrinterMetadata {
                Implementation = typeof(UITextPrinter).FullName,
                LoaderConfiguration = new ResourceLoaderConfiguration { PathPrefix = "Naninovel/TextPrinters" },
            }
        };
    }
}
