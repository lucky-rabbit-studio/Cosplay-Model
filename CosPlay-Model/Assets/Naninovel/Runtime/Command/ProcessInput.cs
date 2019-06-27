// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Allows halting and resuming user input processing (eg, reacting to pressing keyboard keys).
    /// The effect of the action is persistent and saved with the game.
    /// </summary>
    /// <example>
    /// ; Halt input processing
    /// @processInput false
    /// ; Resume input processing
    /// @processInput true
    /// </example>
    public class ProcessInput : Command
    {
        private struct UndoData { public bool Executed; public bool WasEnabled; }

        /// <summary>
        /// Whether to enable input processing.
        /// </summary>
        [CommandParameter(NamelessParameterAlias)]
        public bool InputEnabled { get => GetDynamicParameter(true); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override Task ExecuteAsync ()
        {
            var inputManager = Engine.GetService<InputManager>();

            undoData.Executed = true;
            undoData.WasEnabled = inputManager.ProcessInput;

            inputManager.ProcessInput = InputEnabled;

            return Task.CompletedTask;
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var inputManager = Engine.GetService<InputManager>();
            inputManager.ProcessInput = undoData.WasEnabled;

            undoData = default;
            return Task.CompletedTask;
        }
    }
}
