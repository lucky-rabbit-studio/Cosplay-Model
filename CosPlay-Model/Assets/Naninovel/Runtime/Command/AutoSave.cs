// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Automatically save the game to a quick save slot.
    /// </summary>
    /// <example>
    /// @save
    /// </example>
    [CommandAlias("save")]
    public class AutoSave : Command
    {
        public override async Task ExecuteAsync ()
        {
            await Engine.GetService<StateManager>()?.QuickSaveAsync();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
