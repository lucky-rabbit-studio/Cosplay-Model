// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Next call to <see cref="NovelScriptPlayer.EnableWaitingForInput"/> will be ignored.
    /// </summary>
    /// <example>
    /// ; User won't have to activate a `continue` input in order to progress to the `@sfx` action.
    /// And the rain starts.[skipInput]
    /// @sfx Rain
    /// </example>
    [ActionAlias("skipInput")]
    public class SkipNextWaitForInput : ModifyText
    {
        public override Task ExecuteAsync ()
        {
            Engine.GetService<NovelScriptPlayer>().SkipNextWaitForInput = true;

            return Task.CompletedTask;
        }

        public override Task UndoAsync ()
        {
            Engine.GetService<NovelScriptPlayer>().SkipNextWaitForInput = false;

            return Task.CompletedTask;
        }
    } 
}
