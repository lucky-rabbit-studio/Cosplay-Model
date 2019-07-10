// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Appends provided text to an active printer.
    /// </summary>
    /// <remarks>
    /// The entire text will be appended immediately, without triggering reveal effect or any other side-effects.
    /// </remarks>
    /// <example>
    /// ; Print first part of the sentence as usual (gradually revealing the message),
    /// ; then append the end of the sentence at once.
    /// Lorem ipsum
    /// @append " dolor sit amet."
    /// </example>
    [CommandAlias("append")]
    public class AppendText : ModifyText, Command.ILocalizable
    {
        /// <summary>
        /// The text to append.
        /// </summary>
        [CommandParameter(NamelessParameterAlias)]
        public string Text { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            printer.AppendText(Text);
            printer.PrintedText = printer.PrintedText; // Force update reveal rect.

            Engine.GetService<UIManager>().GetUI<UI.IBacklogUI>()?.AppendMessage(Text);
        }
    } 
}
