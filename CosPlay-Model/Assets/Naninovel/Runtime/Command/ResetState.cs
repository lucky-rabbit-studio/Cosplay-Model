// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Resets state of all the [engine services](https://naninovel.com/guide/engine-services.html) and unloads (disposes) 
    /// all the resources loaded by Naninovel (textures, audio, video, etc); will basically revert to an empty initial engine state.
    /// </summary>
    /// <remarks>
    /// The process is asynchronous and is masked with a loading screen ([ILoadingUI](https://naninovel.com/guide/ui-customization.html)).
    /// <br/><br/>
    /// When [ResetStateOnLoad](https://naninovel.com/guide/configuration.html#state) is disabled in the configuration, you can use this command
    /// to manually dispose unused resources to prevent memory leak issues.
    /// <br/><br/>
    /// Be aware, that this command can not be undone (rewinded back).
    /// </remarks>
    /// <example>
    /// @resetState
    /// </example>
    public class ResetState : Command
    {
        public override async Task ExecuteAsync ()
        {
            await Engine.GetService<StateManager>()?.ResetStateAsync();
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
