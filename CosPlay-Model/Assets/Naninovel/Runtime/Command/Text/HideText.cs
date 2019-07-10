// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Hides an active printer.
    /// </summary>
    /// <example>
    /// @hideText
    /// </example>
    public class HideText : ModifyText
    {
        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            await printer.ChangeVisibilityAsync(false, Duration);
        }
    } 
}
