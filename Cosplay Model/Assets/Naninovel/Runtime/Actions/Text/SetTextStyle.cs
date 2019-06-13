// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Actions
{
    /// <summary>
    /// Applies text style to the active printer.
    /// </summary>
    /// <remarks>
    /// You can still use rich text formatting tags directly, but they will be printed
    /// alongside normal text, which is not desirable in most cases.
    /// </remarks>
    /// <example>
    /// ; Print first sentence in bold red text with 45px size, 
    /// ; then reset the style and print second sentence using default style. 
    /// @style #ff0000,bold,45
    /// Lorem ipsum dolor sit amet.
    /// @style default
    /// Consectetur adipiscing elit.
    /// 
    /// ; Print first sentence normally, but second one in bold and italic;
    /// ; then reset the style to the default.
    /// Lorem ipsum sit amet. [style bold,italic]Consectetur adipiscing elit.[style default]
    /// </example>
    [ActionAlias("style")]
    public class SetTextStyle : ModifyText
    {
        /// <summary>
        /// Text formatting styles to apply.
        /// Possible options: color hex code (eg, #ffaa00), bold, italic, px text size (eg 45).
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string[] TextStyles { get => GetDynamicParameter<string[]>(null); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            if (TextStyles.Length == 1 && TextStyles[0].EqualsFastIgnoreCase("default"))
            {
                printer.RichTextTags = null;
            }
            else
            {
                var tags = new List<string>();
                foreach (var textStyle in TextStyles)
                    tags.Add(TextStyleToRichTextTag(textStyle));
                printer.RichTextTags = tags;
            }
        }

        private string TextStyleToRichTextTag (string textStyle)
        {
            var tag = string.Empty;

            if (textStyle.StartsWithFast("#"))
                tag = "color=" + textStyle;
            else if (textStyle.EqualsFastIgnoreCase("bold"))
                tag = "b";
            else if (textStyle.EqualsFastIgnoreCase("italic"))
                tag = "i";
            else if (Regex.IsMatch(textStyle, @"^\d+$"))
                tag = "size=" + textStyle;

            return tag;
        }
    } 
}
