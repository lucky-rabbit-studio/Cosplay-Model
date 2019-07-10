// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naninovel
{
    /// <summary>
    /// The implementation is an <see cref="IEngineService"/> that is able to manage <see cref="IActor"/> objects.
    /// </summary>
    public interface IActorManager : IEngineService
    {
        /// <summary>
        /// Checks whether an actor with the provided ID is managed by the service. 
        /// </summary>
        bool ActorExists (string actorId);
        /// <summary>
        /// Retrieves a managed actor with the provided ID.
        /// </summary>
        IActor GetActor (string actorId);
        /// <summary>
        /// Retrieves all the actors managed by the service.
        /// </summary>
        IEnumerable<IActor> GetAllActors ();
        /// <summary>
        /// Adds a new managed actor with the provided ID.
        /// </summary>
        Task<IActor> AddActorAsync (string actorId);
        /// <summary>
        /// Adds a new managed actor with the provided state.
        /// </summary>
        Task<IActor> AddActorAsync (ActorState state);
        /// <summary>
        /// Removes a managed actor with the provided ID.
        /// </summary>
        void RemoveActor (string actorId);
        /// <summary>
        /// Removes all the actors managed by the service.
        /// </summary>
        void RemoveAllActors ();
        /// <summary>
        /// Retrieves state of a managed actor with the provided ID.
        /// </summary>
        ActorState GetActorState (string actorId);
        /// <summary>
        /// Retrieves metadata of a managed actor with the provided ID.
        /// </summary>
        ActorMetadata GetActorMetadata (string actorId);
    }
}
