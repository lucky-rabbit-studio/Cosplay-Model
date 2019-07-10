// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Marks a branch of a conditional execution block,
    /// which is executed in case own condition is met (expression is evaluated to be true), while conditions of the opening [`@if`](/api/#if) 
    /// and all the preceding [`@elseif`](/api/#elseif) (if any) commands are not met.
    /// For usage examples see [conditional execution](/guide/naninovel-scripts.md#conditional-execution) guide.
    /// </summary>
    public class ElseIf : Command
    {
        /// <summary>
        /// A [script expression](/guide/script-expressions.md), which should return a boolean value. 
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string Expression { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        public override Task ExecuteAsync ()
        {
            // We might get here either on exiting from an @if or other @elseif branch (which condition is met), or via direct @goto playback jump. 
            // In any case, we just need to get out of the current conditional block.
            BeginIf.HandleConditionalBlock(true);

            return Task.CompletedTask;
        }

        public override Task UndoAsync () => Task.CompletedTask;

        public bool EvaluateExpression () => ExpressionEvaluator.Evaluate<bool>(Expression, LogEvalError);

        private void LogEvalError (string desc = null) => Debug.LogError($"Failed to evaluate conditional (`@elseif`) expression `{Expression}` at script `{ScriptName}` line #{LineNumber}. {desc ?? string.Empty}");
    }
}
