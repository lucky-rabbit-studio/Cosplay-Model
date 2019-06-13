// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;

namespace Naninovel
{
    /// <summary>
    /// Manages choice handler actors.
    /// </summary>
    [InitializeAtRuntime]
    public class ChoiceHandlerManager : NovelActorManager<IChoiceHandlerActor, ChoiceHandlerState>
    {
        public string DefaultHandlerId => config.DefaultHandlerId;

        private readonly ChoiceHandlersConfiguration config;

        public ChoiceHandlerManager (ChoiceHandlersConfiguration config) 
            : base(config)
        {
            this.config = config;
        }

        /// <summary>
        /// Returns currently active handler.
        /// </summary>
        public IChoiceHandlerActor GetActiveHandler ()
        {
            return ManagedActors.Values.FirstOrDefault(p => p.IsHandlerActive);
        }

        /// <summary>
        /// Returns currently active handler; will create and return <see cref="DefaultHandlerId"/> if none.
        /// </summary>
        public async Task<IChoiceHandlerActor> GetActiveHandlerOrDefaultAsync ()
        {
            var activeHandler = ManagedActors.Values.FirstOrDefault(p => p.IsHandlerActive);
            if (activeHandler != null) return activeHandler;
            return await SetActiveHandlerAsync(DefaultHandlerId);
        }

        /// <summary>
        /// Selects choice handler with provided ID as active.
        /// Will de-activate any other handlers on scene.
        /// Will add new actor with provided name if it doesn't exist.
        /// </summary>
        public async Task<IChoiceHandlerActor> SetActiveHandlerAsync (string id)
        {
            DeactivateAllHandlers();

            var handler = await GetOrAddActorAsync(id);
            handler.IsHandlerActive = true;
            return handler;
        }

        /// <summary>
        /// De-activates all the managed choice handlers.
        /// </summary>
        public void DeactivateAllHandlers ()
        {
            foreach (var handler in ManagedActors.Values)
                handler.IsHandlerActive = false;
        }

        public override TMetadata GetActorMetadata<TMetadata> (string actorId) =>
            (config.Metadata.GetMetaById(actorId) ?? config.DefaultMetadata) as TMetadata;
    }
}
