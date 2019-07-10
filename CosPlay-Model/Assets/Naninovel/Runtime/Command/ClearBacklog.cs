// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Removes all the messages from [printer backlog](/guide/printer-backlog.md).
    /// </summary>
    /// <example>
    /// @clearBacklog
    /// </example>
    public class ClearBacklog : Command
    {
        public override Task ExecuteAsync ()
        {
            Engine.GetService<UIManager>()?.GetUI<UI.IBacklogUI>()?.Clear();
            return Task.CompletedTask;
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
