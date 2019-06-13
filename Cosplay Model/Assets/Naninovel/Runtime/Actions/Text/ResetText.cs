// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Clears printed text of active printer.
    /// </summary>
    /// <example>
    /// @resetText
    /// </example>
    public class ResetText : ModifyText
    {
        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            printer.ResetText();
        }
    } 
}
