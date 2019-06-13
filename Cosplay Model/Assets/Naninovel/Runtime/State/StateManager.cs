// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Handles <see cref="IEngineService"/>-related and other engine persistent data de-/serialization.
    /// Provides API to save and load game state.
    /// </summary>
    [InitializeAtRuntime(1)] // Here settings for all the other services will be applied, so initialize at the end.
    public class StateManager : IEngineService
    {
        /// <summary>
        /// Invoked before <see cref="SaveGameAsync(string)"/> execution.
        /// You can use <see cref="GameSaveLoadArgs.StateMap"/> to serialize arbitrary custom objects to the game save slot.
        /// </summary>
        public event Action<GameSaveLoadArgs> OnGameSaveStarted;
        /// <summary>
        /// Invoked after <see cref="SaveGameAsync(string)"/> execution.
        /// </summary>
        public event Action<GameSaveLoadArgs> OnGameSaveFinished;
        /// <summary>
        /// Invoked before <see cref="LoadGameAsync(string)"/> execution.
        /// </summary>
        public event Action<GameSaveLoadArgs> OnGameLoadStarted;
        /// <summary>
        /// Invoked after <see cref="LoadGameAsync(string)"/> execution.
        /// You can use <see cref="GameSaveLoadArgs.StateMap"/> to deserialize previously serialized custom objects from the loaded game save slot.
        /// </summary>
        public event Action<GameSaveLoadArgs> OnGameLoadFinished;
        /// <summary>
        /// Invoked when any state loading operation is started;
        /// eg, loading a saved game or resetting the engine state when returning to the title screen.
        /// </summary>
        public event Action OnLoadStarted;
        /// <summary>
        /// Invoked when any state loading operation is finished;
        /// eg, loading a saved game or resetting the engine state when returning to the title screen.
        /// </summary>
        public event Action OnLoadFinished;

        public GlobalStateMap GlobalState { get; private set; }
        public SettingsStateMap SettingsState { get; private set; }
        public GameStateSlotManager GameStateSlotManager { get; }
        public GlobalStateSlotManager GlobalStateSlotManager { get; }
        public SettingsSlotManager SettingsSlotManager { get; }
        public string LastQuickSaveSlotId => GameStateSlotManager.IndexToQuickSaveSlotId(1);
        public bool QuickLoadAvailable => GameStateSlotManager.SaveSlotExists(LastQuickSaveSlotId);
        public bool AnyGameSaveExists => GameStateSlotManager.AnySaveExists();

        private readonly StateConfiguration config;

        public StateManager (StateConfiguration config, EngineConfiguration engineConfig)
        {
            this.config = config;
            var savesFolderPath = PathUtils.Combine(engineConfig.GeneratedDataPath, config.SaveFolderName);
            GameStateSlotManager = new GameStateSlotManager(savesFolderPath, config.SaveSlotMask, config.QuickSaveSlotMask, config.SaveSlotLimit, config.QuickSaveSlotLimit);
            GlobalStateSlotManager = new GlobalStateSlotManager(savesFolderPath, config.DefaultGlobalSlotId);
            SettingsSlotManager = new SettingsSlotManager(engineConfig.GeneratedDataPath, config.DefaultSettingsSlotId);
        }

        public async Task InitializeServiceAsync ()
        {
            SettingsState = await LoadSettingsAsync();
            GlobalState = await LoadGlobalStateAsync();
        }

        public void ResetService () { }

        public void DestroyService () { }

        /// <summary>
        /// Saves current game state to the specified save slot.
        /// </summary>
        public async Task<GameStateMap> SaveGameAsync (string slotId)
        {
            var state = new GameStateMap();

            OnGameSaveStarted?.Invoke(new GameSaveLoadArgs(slotId, false, state));

            state.SaveDateTime = DateTime.Now;
            state.Thumbnail = Engine.GetService<CameraManager>().CaptureThumbnail();
            await SaveAllServicesToStateAsync<IStatefulService<GameStateMap>, GameStateMap>(state);
            await GameStateSlotManager.SaveAsync(slotId, state);

            // Also save global state on every game save.
            await SaveGlobalStateAsync();

            OnGameSaveFinished?.Invoke(new GameSaveLoadArgs(slotId, false, state));

            return state;
        }

        /// <summary>
        /// Saves current game state to the first quick save slot.
        /// Will shift the quick save slots chain by one index before saving.
        /// </summary>
        public async Task<GameStateMap> QuickSaveAsync ()
        {
            GameStateSlotManager.ShiftQuickSaveSlots();
            var firstSlotId = string.Format(config.QuickSaveSlotMask, 1);

            var state = new GameStateMap();

            OnGameSaveStarted?.Invoke(new GameSaveLoadArgs(firstSlotId, true, state));

            state.SaveDateTime = DateTime.Now;
            state.Thumbnail = Engine.GetService<CameraManager>().CaptureThumbnail();
            await SaveAllServicesToStateAsync<IStatefulService<GameStateMap>, GameStateMap>(state);
            await GameStateSlotManager.SaveAsync(firstSlotId, state);

            // Also save global state on every game save.
            await SaveGlobalStateAsync();

            OnGameSaveFinished?.Invoke(new GameSaveLoadArgs(firstSlotId, true, state));

            return state;
        }

        /// <summary>
        /// Loads game state from the specified save slot.
        /// Will reset the engine services and unload unused assets before load.
        /// </summary>
        public async Task<GameStateMap> LoadGameAsync (string slotId)
        {
            if (!GameStateSlotManager.SaveSlotExists(slotId))
            {
                Debug.LogError($"Slot '{slotId}' not found when loading '{typeof(GameStateMap)}' data.");
                return null;
            }

            OnLoadStarted?.Invoke();
            OnGameLoadStarted?.Invoke(new GameSaveLoadArgs(slotId, false, null));

            if (config.LoadStartDelay > 0)
                await new WaitForSeconds(config.LoadStartDelay);

            Engine.Reset();
            await Resources.UnloadUnusedAssets();

            var state = await GameStateSlotManager.LoadAsync(slotId) as GameStateMap;
            await LoadAllServicesFromStateAsync<IStatefulService<GameStateMap>, GameStateMap>(state);

            OnLoadFinished?.Invoke();
            OnGameLoadFinished?.Invoke(new GameSaveLoadArgs(slotId, false, state));

            return state;
        }

        /// <summary>
        /// Loads game state from the most recent quick save slot.
        /// </summary>
        public async Task<GameStateMap> QuickLoadAsync ()
        {
            if (!GameStateSlotManager.SaveSlotExists(LastQuickSaveSlotId))
            {
                Debug.LogError($"Slot '{LastQuickSaveSlotId}' not found when quick-loading '{typeof(GameStateMap)}' data.");
                return null;
            }

            OnLoadStarted?.Invoke();
            OnGameLoadStarted?.Invoke(new GameSaveLoadArgs(LastQuickSaveSlotId, true, null));

            if (config.LoadStartDelay > 0)
                await new WaitForSeconds(config.LoadStartDelay);

            Engine.Reset();
            await Resources.UnloadUnusedAssets();

            var state = await GameStateSlotManager.LoadAsync(LastQuickSaveSlotId) as GameStateMap;
            await LoadAllServicesFromStateAsync<IStatefulService<GameStateMap>, GameStateMap>(state);

            OnLoadFinished?.Invoke();
            OnGameLoadFinished?.Invoke(new GameSaveLoadArgs(LastQuickSaveSlotId, true, state));

            return state;
        }

        /// <summary>
        /// Loads a default (empty) engine state.
        /// Will reset the engine services and unload unused assets before load.
        /// </summary>
        /// <param name="additionalTasks">Additional tasks to perform while loading.</param>
        public async Task LoadDefaultEngineStateAsync (params Func<Task>[] additionalTasks)
        {
            OnLoadStarted?.Invoke();

            if (config.LoadStartDelay > 0)
                await new WaitForSeconds(config.LoadStartDelay);

            Engine.Reset();
            await Resources.UnloadUnusedAssets();

            if (additionalTasks != null)
            {
                foreach (var task in additionalTasks)
                    await task?.Invoke();
            }

            OnLoadFinished?.Invoke();
        }

        public async Task<GlobalStateMap> SaveGlobalStateAsync ()
        {
            await SaveAllServicesToStateAsync<IStatefulService<GlobalStateMap>, GlobalStateMap>(GlobalState);
            await GlobalStateSlotManager.SaveAsync(config.DefaultGlobalSlotId, GlobalState);
            return GlobalState;
        }

        public async Task<SettingsStateMap> SaveSettingsAsync ()
        {
            await SaveAllServicesToStateAsync<IStatefulService<SettingsStateMap>, SettingsStateMap>(SettingsState);
            await SettingsSlotManager.SaveAsync(config.DefaultSettingsSlotId, SettingsState);
            return SettingsState;
        }

        private async Task<GlobalStateMap> LoadGlobalStateAsync ()
        {
            var stateData = await GlobalStateSlotManager.LoadOrDefaultAsync(config.DefaultGlobalSlotId);
            await LoadAllServicesFromStateAsync<IStatefulService<GlobalStateMap>, GlobalStateMap>(stateData);
            return stateData;
        }

        private async Task<SettingsStateMap> LoadSettingsAsync ()
        {
            var settingsData = await SettingsSlotManager.LoadOrDefaultAsync(config.DefaultSettingsSlotId);
            await LoadAllServicesFromStateAsync<IStatefulService<SettingsStateMap>, SettingsStateMap>(settingsData);
            return settingsData;
        }

        private async Task SaveAllServicesToStateAsync<TService, TState> (TState state) 
            where TService : class, IStatefulService<TState>
            where TState : StateMap, new()
        {
            foreach (var service in Engine.GetAllServices<TService>())
                await service.SaveServiceStateAsync(state);
        }

        private async Task LoadAllServicesFromStateAsync<TService, TState> (TState state)
            where TService : class, IStatefulService<TState>
            where TState : StateMap, new()
        {
            foreach (var service in Engine.GetAllServices<TService>())
                await service.LoadServiceStateAsync(state);
        }
    } 
}
