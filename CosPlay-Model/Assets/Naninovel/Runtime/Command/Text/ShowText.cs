// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Shows an active or default text printer.
    /// </summary>
    /// <example>
    /// @showText
    /// </example>
    public class ShowText : ModifyText
    {
        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            await printer.ChangeVisibilityAsync(true, Duration);
        }
    } 
}
