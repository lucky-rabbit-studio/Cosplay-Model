// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Will execute [`@goto`](/api/#goto) and/or [`@set`](/api/#set) actions when the provided 
    /// [script expression](/guide/script-expressions.md) is evaluated to `true` boolean value.
    /// </summary>
    /// <example>
    /// ; Given a `score` variable is set (eg, via `@set` action) to an integer value:
    /// 
    /// ; Play a `fanfare` SFX for `score` times.
    /// @set counter=1
    /// # FanfareLoop
    /// @sfx Fanfare
    /// @if counter&lt;score set:counter=counter+1 goto:.FanfareLoop
    /// 
    /// ; Set variable `mood` to `Great` if `score` is equal to or greater than 5,
    /// ; to `Fine` if 4 or greater, and to `Average` in the other cases. 
    /// @if score>=5 set:mood="Great"
    /// @if "score >= 4 &amp;&amp; score &lt; 5" set:mood="Fine"
    /// @if score&lt;4 set:mood="Average"
    /// 
    /// ; You can also use `if` parameter on other actions to conditionally execute them:
    /// 
    /// ; If `level` value is a number and is greater than 9000, add the choice
    /// @choice "It's over 9000!" if:level>9000
    /// 
    /// ; If `dead` variable is a bool and equal to `false`, execute the print action
    /// @print text:"I'm still alive." if:!dead
    /// 
    /// ; If `glitch` is a bool and equals `true` or random function in 1 to 10 range returns 5 or more, execute `@fx` action
    /// @fx GlitchCamera if:"glitch || Random(1, 10) >= 5"
    /// 
    /// ; If `score` value is in 7 to 13 range or `lucky` variable is a bool and equals `true`, load `LuckyEnd` script
    /// @goto LuckyEnd if:"(score >= 7 &amp;&amp; score &lt;= 13) || lucky"
    /// 
    /// ; You can also use conditionals in the inlined actions
    /// Lorem sit amet. [style bold if:score>=10]Consectetur elit.[style default]
    /// 
    /// ; When using double quotes inside the expression itself, don't forget to double-escape them
    /// @print {remark} if:remark=="Saying \\"Stop the car\\" was a mistake."
    /// </example>
    [ActionAlias("if")]
    public class ConditionalFlow : NovelAction
    {
        private struct UndoData { public SetCustomVariable SetAction; }

        /// <summary>
        /// A [script expression](/guide/script-expressions.md), which should return a boolean value. 
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string Expression { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Path to go when expression is true; see [`@goto`](/api/#goto) action for the path format.
        /// </summary>
        [ActionParameter("goto", true)]
        public Named<string> GotoPath { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Set expression to execute when the conditional expression is true; see [`@set`](/api/#set) action for syntax reference.
        /// </summary>
        [ActionParameter("set", true)]
        public string SetExpression { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            if (string.IsNullOrEmpty(Expression) || (GotoPath is null && string.IsNullOrEmpty(SetExpression)))
            {
                Debug.LogWarning($"Empty conditional expression at `{ScriptName}` script at line #{LineNumber}.");
                return;
            }

            var result = ExpressionEvaluator.Evaluate<bool>(Expression, LogErrorMsg);
            if (!result) return;

            if (!string.IsNullOrEmpty(SetExpression))
            {
                var setAction = new SetCustomVariable { Expression = SetExpression };
                undoData.SetAction = setAction;
                await setAction.ExecuteAsync();
            }
            if (GotoPath != null) await new Goto { Path = GotoPath }.ExecuteAsync();
        }

        public override async Task UndoAsync ()
        {
            if (undoData.SetAction != null)
                await undoData.SetAction.UndoAsync();

            undoData = default;
        }

        private void LogErrorMsg (string desc = null) => Debug.LogError($"Failed to evaluate conditional expression `{Expression}` at `{ScriptName}` script at line #{LineNumber}. {desc ?? string.Empty}");
    }
}
