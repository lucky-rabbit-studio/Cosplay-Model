// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Marks a branch of a conditional execution block,
    /// which is always executed in case conditions of the opening [`@if`](/api/#if) and all the preceding [`@elseif`](/api/#elseif) (if any) commands are not met.
    /// For usage examples see [conditional execution](/guide/naninovel-scripts.md#conditional-execution) guide.
    /// </summary>
    public class Else : Command
    {
        public override Task ExecuteAsync ()
        {
            // We might get here either on exiting from an @if or @elseif branch (which condition is met), or via direct @goto playback jump. 
            // In any case, we just need to get out of the current conditional block.
            BeginIf.HandleConditionalBlock(true);

            return Task.CompletedTask;
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
