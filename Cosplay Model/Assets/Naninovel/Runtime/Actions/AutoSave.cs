// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Automatically save the game to a quick save slot.
    /// </summary>
    /// <example>
    /// @save
    /// </example>
    [ActionAlias("save")]
    public class AutoSave : NovelAction
    {
        public override async Task ExecuteAsync ()
        {
            await Engine.GetService<StateManager>()?.QuickSaveAsync();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
