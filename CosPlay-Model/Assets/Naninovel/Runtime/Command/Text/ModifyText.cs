// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    public abstract class ModifyText : Command
    {
        protected struct UndoActionData { public bool Executed, AddedActor; public string InitialActivePrinterId; public TextPrinterState State; }

        protected UndoActionData UndoData;

        public override Task UndoAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();

            if (!UndoData.Executed) return Task.CompletedTask;

            if (UndoData.State != null)
            {
                var actor = mngr.GetActor(UndoData.State.Id);
                if (actor != null) UndoData.State.ApplyToActor(actor);
            }

            if (!string.IsNullOrWhiteSpace(UndoData.InitialActivePrinterId))
                mngr.SetActivePrinter(UndoData.InitialActivePrinterId);
            else mngr.DeactivateAllPrinters();

            UndoData = default;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns currently active printer; when no active printers found, will add a default one and make it active.
        /// Also registers all the state mutations to the <see cref="UndoData"/>.
        /// </summary>
        protected async Task<ITextPrinterActor> GetActivePrinterOrDefaultWithUndoAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();

            UndoData.InitialActivePrinterId = mngr.GetActivePrinter()?.Id;
            UndoData.AddedActor = mngr.GetActivePrinter() is null && !mngr.ActorExists(mngr.DefaultPrinterId);
            var printer = UndoData.AddedActor ? await mngr.AddActorAsync(mngr.DefaultPrinterId) : mngr.GetActivePrinter() ?? mngr.GetActor(mngr.DefaultPrinterId);
            if (string.IsNullOrEmpty(UndoData.InitialActivePrinterId)) mngr.SetActivePrinter(printer?.Id);
            return printer;
        }
    }
}
