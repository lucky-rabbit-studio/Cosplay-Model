// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Next call to <see cref="ScriptPlayer.EnableWaitingForInput"/> will be ignored.
    /// </summary>
    /// <example>
    /// ; User won't have to activate a `continue` input in order to progress to the `@sfx` command.
    /// And the rain starts.[skipInput]
    /// @sfx Rain
    /// </example>
    [CommandAlias("skipInput")]
    public class SkipNextWaitForInput : ModifyText
    {
        public override Task ExecuteAsync ()
        {
            Engine.GetService<ScriptPlayer>().SkipNextWaitForInput = true;

            return Task.CompletedTask;
        }

        public override Task UndoAsync ()
        {
            Engine.GetService<ScriptPlayer>().SkipNextWaitForInput = false;

            return Task.CompletedTask;
        }
    } 
}
