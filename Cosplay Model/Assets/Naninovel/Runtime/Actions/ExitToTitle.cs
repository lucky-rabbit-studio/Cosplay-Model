// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Loads default engine state and shows <see cref="UI.ITitleUI"/>.
    /// </summary>
    /// <example>
    /// @title
    /// </example>
    [ActionAlias("title")]
    public class ExitToTitle : NovelAction
    {
        public override async Task ExecuteAsync ()
        {
            var gameState = Engine.GetService<StateManager>();
            var uiManager = Engine.GetService<UIManager>();

            await gameState.LoadDefaultEngineStateAsync();
            uiManager.GetUI<UI.ITitleUI>()?.Show();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
