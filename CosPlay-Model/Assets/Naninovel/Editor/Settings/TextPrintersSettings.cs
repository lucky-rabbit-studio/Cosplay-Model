// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityCommon;
using UnityEditor;

namespace Naninovel
{
    public class TextPrintersSettings : ActorManagerSettings<TextPrintersConfiguration, ITextPrinterActor, TextPrinterMetadata>
    {
        protected override string HelpUri => "guide/text-printers.html";
        protected override Type ResourcesTypeConstraint => GetTypeConstraint();
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override bool AllowMultipleResources => false;
        protected override HashSet<string> LockedActorIds => new HashSet<string> { "Dialogue", "Fullscreen", "Wide", "Chat" };

        private Type GetTypeConstraint ()
        {
            switch (EditedMetadata?.Implementation?.GetAfter("."))
            {
                case nameof(UITextPrinter): return typeof(UI.UITextPrinterPanel);
                default: return null;
            }
        }

        private string GetTooltip ()
        {
            if (EditedActorId == Configuration.DefaulPrinterId)
                return "This printer will be active by default: all the generic text and `@print` commands will use it to output the text. Use `@printer PrinterID` action to change active printer.";
            return $"Use `@printer {EditedActorId}` in naninovel scripts to set this printer active; all the consequent generic text and `@print` commands will then use it to output the text.";
        }

        [MenuItem("Naninovel/Resources/Text Printers")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
