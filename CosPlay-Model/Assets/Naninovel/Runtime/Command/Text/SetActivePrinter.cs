// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Sets printer with the provided ID active and de-activates all the others.
    /// </summary>
    /// <example>
    /// ; Will activate `Dialogue` printer
    /// @printer Dialogue
    /// 
    /// ; Will active `Fullscreen` printer
    /// @printer Fullscreen
    /// </example>
    [CommandAlias("printer")]
    public class SetActivePrinter : ModifyText, Command.IPreloadable
    {
        /// <summary>
        /// ID of the printer to activate.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string PrinterId { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        public async Task HoldResourcesAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await mngr.GetOrAddActorAsync(PrinterId);
            await printer.HoldResourcesAsync(this, null);
        }

        public void ReleaseResources ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            if (mngr is null || !mngr.ActorExists(PrinterId)) return;
            mngr?.GetActor(PrinterId).ReleaseResources(this, null);
        }

        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();

            UndoData.Executed = true;
            UndoData.InitialActivePrinterId = mngr.GetActivePrinter()?.Id;
            UndoData.AddedActor = !mngr.ActorExists(PrinterId);

            if (!mngr.ActorExists(PrinterId))
                await mngr.AddActorAsync(PrinterId);
            mngr.SetActivePrinter(PrinterId);
        }
    } 
}
