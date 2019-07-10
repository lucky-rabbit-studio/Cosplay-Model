// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Stops the naninovel script execution.
    /// </summary>
    /// <example>
    /// @stop
    /// </example>
    public class Stop : Command
    {
        public override Task ExecuteAsync ()
        {
            Engine.GetService<ScriptPlayer>().Stop();

            return Task.CompletedTask;
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
