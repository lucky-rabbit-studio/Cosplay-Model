// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Loads default engine state and shows <see cref="UI.ITitleUI"/>.
    /// </summary>
    /// <example>
    /// @title
    /// </example>
    [CommandAlias("title")]
    public class ExitToTitle : Command
    {
        public override async Task ExecuteAsync ()
        {
            var gameState = Engine.GetService<StateManager>();
            var uiManager = Engine.GetService<UIManager>();

            await gameState.ResetStateAsync();
            uiManager.GetUI<UI.ITitleUI>()?.Show();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
