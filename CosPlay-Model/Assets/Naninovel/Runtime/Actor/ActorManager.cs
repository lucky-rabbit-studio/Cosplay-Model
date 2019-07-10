// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public abstract class ActorManager<TActor, TState> : IActorManager, IStatefulService<GameStateMap>
        where TActor : IActor
        where TState : ActorState<TActor>, new()
    {
        [Serializable]
        private class GameState
        {
            public List<string> ActorStateJsonList = new List<string>();
        }

        public EasingType DefaultEasingType => config.DefaultEasing;

        protected Dictionary<string, TActor> ManagedActors { get; set; }

        private static IEnumerable<Type> implementationTypes;

        private readonly ActorManagerConfiguration config;
        private readonly Dictionary<string, TaskCompletionSource<TActor>> pendingAddActorTasks;

        static ActorManager ()
        {
            implementationTypes = ReflectionUtils.ExportedDomainTypes
                .Where(t => t.GetInterfaces().Contains(typeof(TActor)));
        }

        public ActorManager (ActorManagerConfiguration config)
        {
            this.config = config;
            ManagedActors = new Dictionary<string, TActor>(StringComparer.Ordinal);
            pendingAddActorTasks = new Dictionary<string, TaskCompletionSource<TActor>>();
        }

        public virtual Task InitializeServiceAsync () => Task.CompletedTask;

        public virtual void ResetService ()
        {
            RemoveAllActors();
        }

        public virtual void DestroyService ()
        {
            RemoveAllActors();
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var state = new GameState();
            foreach (var kv in ManagedActors)
            {
                var actorState = new TState();
                actorState.OverwriteFromActor(kv.Value);
                state.ActorStateJsonList.Add(actorState.ToJson());
            }
            stateMap.SerializeObject(state);
            return Task.CompletedTask;
        }

        public async Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            foreach (var stateJson in state.ActorStateJsonList)
            {
                var actorState = new TState();
                actorState.OverwriteFromJson(stateJson);
                var actor = await GetOrAddActorAsync(actorState.Id);
                actorState.ApplyToActor(actor);
            }
        }

        /// <summary>
        /// Checks whether an actor with the provided ID is managed by the service. 
        /// </summary>
        public bool ActorExists (string actorId) => ManagedActors.ContainsKey(actorId);

        /// <summary>
        /// Adds a new managed actor with the provided ID.
        /// </summary>
        public virtual async Task<TActor> AddActorAsync (string actorId)
        {
            if (ActorExists(actorId))
            {
                Debug.LogWarning($"Actor '{actorId}' was requested to be added, but it already exists.");
                return GetActor(actorId);
            }

            if (pendingAddActorTasks.ContainsKey(actorId))
                return await pendingAddActorTasks[actorId].Task;

            pendingAddActorTasks[actorId] = new TaskCompletionSource<TActor>();

            var constructedActor = await ConstructActorAsync(actorId);
            ManagedActors.Add(actorId, constructedActor);

            pendingAddActorTasks[actorId].SetResult(constructedActor);
            pendingAddActorTasks.Remove(actorId);

            return constructedActor;
        }

        /// <summary>
        /// Adds a new managed actor with the provided ID.
        /// </summary>
        async Task<IActor> IActorManager.AddActorAsync (string actorId) => await AddActorAsync(actorId);

        /// <summary>
        /// Adds a new managed actor with the provided state.
        /// </summary>
        public virtual async Task<TActor> AddActorAsync (TState state)
        {
            if (string.IsNullOrWhiteSpace(state?.Id))
            {
                Debug.LogWarning($"Can't add an actor with '{state}' state: actor name is undefined.");
                return default;
            }

            var actor = await AddActorAsync(state.Id);
            state.ApplyToActor(actor);
            return actor;
        }

        /// <summary>
        /// Adds a new managed actor with the provided state.
        /// </summary>
        public Task<IActor> AddActorAsync (ActorState state) => AddActorAsync(state);

        /// <summary>
        /// Retrieves a managed actor with the provided ID.
        /// </summary>
        public virtual TActor GetActor (string actorId)
        {
            if (!ActorExists(actorId))
            {
                Debug.LogError($"Can't find '{actorId}' actor.");
                return default;
            }

            return ManagedActors[actorId];
        }

        /// <summary>
        /// Retrieves a managed actor with the provided ID.
        /// </summary>
        IActor IActorManager.GetActor (string actorId) => GetActor(actorId);

        /// <summary>
        /// Returns a managed actor with the provided ID. If the actor doesn't exist, will add it.
        /// </summary>
        public virtual async Task<TActor> GetOrAddActorAsync (string actorId) => ActorExists(actorId) ? GetActor(actorId) : await AddActorAsync(actorId);

        /// <summary>
        /// Retrieves all the actors managed by the service.
        /// </summary>
        public virtual IEnumerable<TActor> GetAllActors () => ManagedActors?.Values;

        /// <summary>
        /// Retrieves all the actors managed by the service.
        /// </summary>
        IEnumerable<IActor> IActorManager.GetAllActors () => ManagedActors?.Values.Cast<IActor>();

        /// <summary>
        /// Removes a managed actor with the provided ID.
        /// </summary>
        public virtual void RemoveActor (string actorId)
        {
            if (!ActorExists(actorId)) return;
            var actor = GetActor(actorId);
            ManagedActors.Remove(actor.Id);
            (actor as IDisposable)?.Dispose();
        }

        /// <summary>
        /// Removes all the actors managed by the service.
        /// </summary>
        public virtual void RemoveAllActors ()
        {
            if (ManagedActors.Count == 0) return;
            var managedActors = GetAllActors().ToArray();
            for (int i = 0; i < managedActors.Length; i++)
                RemoveActor(managedActors[i].Id);
            ManagedActors.Clear();
        }

        /// <summary>
        /// Retrieves state of a managed actor with the provided ID.
        /// </summary>
        public virtual TState GetActorState (string actorId)
        {
            if (!ActorExists(actorId))
            {
                Debug.LogError($"Can't find '{actorId}' actor.");
                return default;
            }

            var actor = GetActor(actorId);
            var state = new TState();
            state.OverwriteFromActor(actor);
            return state;
        }

        /// <summary>
        /// Retrieves state of a managed actor with the provided ID.
        /// </summary>
        ActorState IActorManager.GetActorState (string actorId) => GetActorState(actorId);

        /// <summary>
        /// Retrieves metadata of a managed actor with the provided ID.
        /// </summary>
        public abstract ActorMetadata GetActorMetadata (string actorId);

        /// <summary>
        /// Retrieves metadata of a managed actor with the provided ID.
        /// </summary>
        public TMetadata GetActorMetadata<TMetadata> (string actorId) 
            where TMetadata : ActorMetadata => GetActorMetadata(actorId) as TMetadata;

        protected virtual async Task<TActor> ConstructActorAsync (string actorId)
        {
            var metadata = GetActorMetadata<ActorMetadata>(actorId);

            var implementationType = implementationTypes.FirstOrDefault(t => t.FullName == metadata.Implementation);
            Debug.Assert(implementationType != null, $"`{metadata.Implementation}` actor implementation type for `{typeof(TActor).Name}` is not found.");

            var actor = (TActor)Activator.CreateInstance(implementationType, actorId, metadata);

            await actor.InitializeAsync();

            return actor;
        }
    } 
}
