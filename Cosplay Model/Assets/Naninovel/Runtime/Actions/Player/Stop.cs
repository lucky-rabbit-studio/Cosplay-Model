// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Stops the novel script execution.
    /// </summary>
    /// <example>
    /// @stop
    /// </example>
    public class Stop : NovelAction
    {
        public override Task ExecuteAsync ()
        {
            Engine.GetService<NovelScriptPlayer>().Stop();

            return Task.CompletedTask;
        }

        public override Task UndoAsync () => Task.CompletedTask;
    } 
}
