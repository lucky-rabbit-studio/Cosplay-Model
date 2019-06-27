// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Adds a line break to the text in active printer.
    /// </summary>
    /// <example>
    /// ; Second sentence will be printed on a new line
    /// Lorem ipsum dolor sit amet.[br]Consectetur adipiscing elit.
    /// 
    /// ; Second sentence will be printer two lines under the first one
    /// Lorem ipsum dolor sit amet.[br 2]Consectetur adipiscing elit.
    /// </example>
    [CommandAlias("br")]
    public class AppendLineBreak : ModifyText
    {
        /// <summary>
        /// Number of line breaks to add.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias, optional: true)]
        public int Count { get => GetDynamicParameter(1); set => SetDynamicParameter(value); }

        public override async Task ExecuteAsync ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            var printer = await GetActivePrinterOrDefaultWithUndoAsync();
            UndoData.Executed = true;
            UndoData.State = mngr.GetActorState(printer.Id);

            var backlogUI = Engine.GetService<UIManager>()?.GetUI<UI.IBacklogUI>();

            for (int i = 0; i < Count; i++)
            {
                printer.AppendText("\n");
                backlogUI?.AppendMessage("\n");
            }
        }
    } 
}
