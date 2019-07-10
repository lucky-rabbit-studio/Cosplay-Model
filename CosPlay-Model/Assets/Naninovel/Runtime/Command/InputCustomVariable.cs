// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Shows an input field UI where user can enter an arbitrary text.
    /// Upon submit the entered text will be assigned to the specified custom variable.
    /// </summary>
    /// <remarks>
    /// Check out this [video guide](https://youtu.be/F9meuMzvGJw) on usage example.
    /// <br/><br/>
    /// To assign a display name for a character using this command consider using [binding the name to a custom variable](/guide/characters.html#display-names).
    /// <br/><br/>
    /// The state of the UI is not serialized when saving the game, so make sure to prevent 
    /// player from saving the game when the UI is visible (eg, with `@hideText` command).
    /// </remarks>
    /// <example>
    /// ; Allow user to enter an arbitrary text and assign it to `name` custom state variable
    /// @input name summary:"Choose your name."
    /// ; Stop command is required to halt script execution until user submits the input
    /// @stop
    /// 
    /// ; You can then inject the assigned `name` variable in naninovel scripts
    /// Archibald: Greetings, {name}!
    /// {name}: Yo! 
    /// 
    /// ; ...or use it inside set and conditional expressions
    /// @set score=score+1 if:name=="Felix"
    /// </example>
    [CommandAlias("input")]
    public class InputCustomVariable : Command, Command.ILocalizable
    {
        /// <summary>
        /// Name of a custom variable to which the entered text will be assigned.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string VariableName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// An optional summary text to show along with input field.
        /// When the text contain spaces, wrap it in double quotes (`"`). 
        /// In case you wish to include the double quotes in the text itself, escape them.
        /// </summary>
        [CommandParameter(optional: true)]
        public string Summary { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Whether to automatically resume script playback when user submits the input form.
        /// </summary>
        [CommandParameter("play", true)]
        public bool PlayOnSubmit { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }

        private CustomVariableManager VariableMngr => Engine.GetService<CustomVariableManager>();
        private UIManager UIMngr => Engine.GetService<UIManager>();
        private KeyValuePair<string, string> undoData;

        public override Task ExecuteAsync ()
        {
            undoData = new KeyValuePair<string, string>(VariableName, VariableMngr.GetVariableValue(VariableName) ?? string.Empty);

            UIMngr.GetUI<UI.IVariableInputUI>()?.Show(VariableName, Summary, PlayOnSubmit);

            return Task.CompletedTask;
        }

        public override Task UndoAsync ()
        {
            if (string.IsNullOrWhiteSpace(undoData.Key)) return Task.CompletedTask;

            UIMngr.GetUI<UI.IVariableInputUI>()?.Hide();
            VariableMngr.SetVariableValue(undoData.Key, undoData.Value);

            undoData = default;
            return Task.CompletedTask;
        }
    }
}
