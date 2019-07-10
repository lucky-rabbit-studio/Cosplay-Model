// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.Commands
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
    [CommandAlias("movie")]
    public class PlayMovie : Command, Command.IPreloadable
    {
        /// <summary>
        /// Name of the movie resource to play.
        /// </summary>
        [CommandParameter(alias: NamelessParameterAlias)]
        public string MovieName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        protected MoviePlayer Player => Engine.GetService<MoviePlayer>();

        public async Task HoldResourcesAsync ()
        {
            await Player?.HoldResourcesAsync(this, MovieName);
        }

        public void ReleaseResources ()
        {
            Player?.ReleaseResources(this, MovieName);
        }

        public override async Task ExecuteAsync ()
        {
            await Player?.PlayAsync(MovieName);
        }

        public override Task UndoAsync () => Task.CompletedTask;
    }
}
