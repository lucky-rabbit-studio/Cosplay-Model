// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Commands
{
    /// <summary>
    /// Applies [text styles](/guide/text-printers.md#text-styles) to an active printer.
    /// </summary>
    /// <remarks>
    /// You can still use rich text formatting tags directly, but they could be printed
    /// alongside the normal text (eg, when appending messages), which may not be desirable.
    /// </remarks>
    /// <example>
    /// ; Print first sentence in bold red text with 45px size, 
    /// ; then reset the style and print second sentence using default style. 
    /// @style color=#ff0000,b,size=45
    /// Lorem ipsum dolor sit amet.
    /// @style default
    /// Consectetur adipiscing elit.
    /// 
    /// ; Print first sentence normally, but second one in bold and italic;
    /// ; then reset the style to the default.
    /// Lorem ipsum sit amet. [style b,i]Consectetur adipiscing elit.[style default]
    /// </example>
    [CommandAlias("style")]
    public class SetTextStyle : ModifyText
    {
        /// <summary>
        /// Text formatting tags to apply. Angle brackets should be ommited, eg use `b` for &lt;b&gt; and `size=100` for &lt;size=100&gt;. Use `default` keyword to reset the style.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
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
            else printer.RichTextTags = TextStyles?.ToList();
        }
    } 
}
