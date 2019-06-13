// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public abstract class NovelActorManager : IStatefulService<GameStateMap>
    {
        public EasingType DefaultEasingType => config.DefaultEasing;

        private readonly NovelActorManagerConfiguration config;

        public NovelActorManager (NovelActorManagerConfiguration config)
        {
            this.config = config;
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

        public abstract bool ActorExists (string actorId);
        public INovelActor GetActor (string actorId) => GetNovelActor(actorId);
        public NovelActorState GetActorState (string actorId) => GetNovelActorState(actorId);
        public IEnumerable<INovelActor> GetAllActors () => GetAllNovelActors();
        public Task<INovelActor> AddActorAsync (string actorId) => AddNovelActorAsync(actorId);
        public async Task<INovelActor> GetOrAddActorAsync (string actorId) => ActorExists(actorId) ? GetActor(actorId) : await AddActorAsync(actorId);
        public abstract TMetadata GetActorMetadata<TMetadata> (string actorId) where TMetadata : NovelActorMetadata;
        public abstract Task<INovelActor> AddActorAsync (NovelActorState state);
        public abstract void RemoveActor (string actorId);
        public abstract void RemoveAllActors ();
        public abstract Task SaveServiceStateAsync (GameStateMap engineGameState);
        public abstract Task LoadServiceStateAsync (GameStateMap engineGameState);

        protected abstract INovelActor GetNovelActor (string actorId);
        protected abstract NovelActorState GetNovelActorState (string actorId);
        protected abstract IEnumerable<INovelActor> GetAllNovelActors ();
        protected abstract Task<INovelActor> AddNovelActorAsync (string actorId);
    }

    public abstract class NovelActorManager<TActor, TState> : NovelActorManager 
        where TActor : INovelActor
        where TState : NovelActorState<TActor>, new()
    {
        [Serializable]
        private class GameState
        {
            public List<string> ActorStateJsonList = new List<string>();
        }

        protected Dictionary<string, TActor> ManagedActors { get; set; }

        private static IEnumerable<Type> implementationTypes;

        static NovelActorManager ()
        {
            implementationTypes = ReflectionUtils.ExportedDomainTypes
                .Where(t => t.GetInterfaces().Contains(typeof(TActor)));
        }

        public NovelActorManager (NovelActorManagerConfiguration config) 
            : base(config)
        {
            ManagedActors = new Dictionary<string, TActor>(StringComparer.Ordinal);
        }

        public override bool ActorExists (string actorId)
        {
            return ManagedActors.ContainsKey(actorId);
        }

        /// <summary>
        /// Adds a new managed actor with the provided name.
        /// </summary>
        public new virtual async Task<TActor> AddActorAsync (string actorId)
        {
            if (ActorExists(actorId))
            {
                Debug.LogWarning($"Actor '{actorId}' was requested to be added, but it already exists.");
                return GetActor(actorId);
            }

            var constructedActor = await ConstructActorAsync(actorId);
            ManagedActors.Add(actorId, constructedActor);

            return constructedActor;
        }

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
        public override Task<INovelActor> AddActorAsync (NovelActorState state) => AddActorAsync(state);

        public new virtual TActor GetActor (string actorId)
        {
            if (!ActorExists(actorId))
            {
                Debug.LogError($"Can't find '{actorId}' actor.");
                return default;
            }

            return ManagedActors[actorId];
        }

        /// <summary>
        /// Returns a managed actor with the provided name. If the actor doesn't exist, will add it.
        /// </summary>
        public new virtual async Task<TActor> GetOrAddActorAsync (string actorId) => ActorExists(actorId) ? GetActor(actorId) : await AddActorAsync(actorId);

        public new virtual TState GetActorState (string actorId)
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

        public new virtual IEnumerable<TActor> GetAllActors () => ManagedActors?.Values;

        public override void RemoveActor (string actorId)
        {
            if (!ActorExists(actorId)) return;
            var actor = GetActor(actorId);
            ManagedActors.Remove(actor.Id);
            (actor as IDisposable)?.Dispose();
        }

        public override void RemoveAllActors ()
        {
            if (ManagedActors.Count == 0) return;
            var managedActors = GetAllActors().ToArray();
            for (int i = 0; i < managedActors.Length; i++)
                RemoveActor(managedActors[i].Id);
            ManagedActors.Clear();
        }

        public override Task SaveServiceStateAsync (GameStateMap stateMap)
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

        public override async Task LoadServiceStateAsync (GameStateMap stateMap)
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

        protected virtual async Task<TActor> ConstructActorAsync (string actorId)
        {
            var metadata = GetActorMetadata<NovelActorMetadata>(actorId);

            var implementationType = implementationTypes.FirstOrDefault(t => t.FullName == metadata.Implementation);
            Debug.Assert(implementationType != null, $"`{metadata.Implementation}` actor implementation type for `{typeof(TActor).Name}` is not found.");

            var actor = (TActor)Activator.CreateInstance(implementationType, actorId, metadata);

            await actor.InitializeAsync();

            return actor;
        }

        protected override INovelActor GetNovelActor (string actorId) => GetActor(actorId);
        protected override NovelActorState GetNovelActorState (string actorId) => GetActorState(actorId);
        protected override IEnumerable<INovelActor> GetAllNovelActors () => GetAllActors().Cast<INovelActor>();
        protected override async Task<INovelActor> AddNovelActorAsync (string actorId) => await AddActorAsync(actorId);
    } 
}
