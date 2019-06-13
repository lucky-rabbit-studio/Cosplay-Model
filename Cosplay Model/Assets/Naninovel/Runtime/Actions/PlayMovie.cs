// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Playes a movie with the provided name (path).
    /// </summary>
    /// <remarks>
    /// Will fade-out the screen before playing the movie and fade back in after the play.
    /// Playback can be canceled by activating a `cancel` input (`Esc` key by default).
    /// </remarks>
    /// <example>
    /// ; Given an "Opening" video clip is added to the movie resources, plays it
    /// @movie Opening
    /// </example>
    [ActionAlias("movie")]
    public class PlayMovie : NovelAction, NovelAction.IPreloadable
    {
        /// <summary>
        /// Name of the movie resource to play.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string MovieName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        protected MoviePlayer Player => Engine.GetService<MoviePlayer>();

        public async Task PreloadResourcesAsync ()
        {
            await Player?.PreloadAsync(MovieName);
        }

        public async Task UnloadResourcesAsync ()
        {
            await Player?.UnloadAsync(MovieName);
        }

        public override async Task ExecuteAsync ()
        {
            await Player?.PlayAsync(MovieName);
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
