// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Naninovel
{
    /// <summary>
    /// Manages the user input processing.
    /// </summary>
    [InitializeAtRuntime]
    public class InputManager : IStatefulService<GameStateMap>
    {
        [System.Serializable]
        private class GameState
        {
            public bool ProcessInput = true;
        }

        public const string SubmitName = "Submit";
        public const string CancelName = "Cancel";
        public const string ContinueName = "Continue";
        public const string SkipName = "Skip";
        public const string AutoPlayName = "AutoPlay";
        public const string ToggleUIName = "ToggleUI";
        public const string ShowBacklogName = "ShowBacklog";

        public InputSampler Submit => GetSampler(SubmitName);
        public InputSampler Cancel => GetSampler(CancelName);
        public InputSampler Continue => GetSampler(ContinueName);
        public InputSampler Skip => GetSampler(SkipName);
        public InputSampler AutoPlay => GetSampler(AutoPlayName);
        public InputSampler ToggleUI => GetSampler(ToggleUIName);
        public InputSampler ShowBacklog => GetSampler(ShowBacklogName);

        public bool ProcessInput { get; set; } = true;

        private readonly InputConfiguration config;
        private Dictionary<string, InputSampler> samplersMap;
        private IEngineBehaviour engineBehaviour;
        private GameObject gameObject;
        private Dictionary<IManagedUI, string[]> blockingUIs;
        private HashSet<string> blockedSamplers;

        public InputManager (InputConfiguration config, IEngineBehaviour engineBehaviour)
        {
            this.config = config;
            this.engineBehaviour = engineBehaviour;
            samplersMap = new Dictionary<string, InputSampler>(System.StringComparer.Ordinal);
            blockingUIs = new Dictionary<IManagedUI, string[]>();
            blockedSamplers = new HashSet<string>();
        }

        public Task InitializeServiceAsync ()
        {
            foreach (var binding in config.Bindings)
            {
                var sampler = new InputSampler(binding, null, config.TouchContinueCooldown);
                samplersMap[binding.Name] = sampler;
            }

            if (config.SpawnEventSystem)
                gameObject = Engine.CreateObject("InputManager", -1, typeof(EventSystem), typeof(StandaloneInputModule));

            engineBehaviour.OnBehaviourUpdate += SampleInput;

            return Task.CompletedTask;
        }

        public void ResetService () { }

        public void DestroyService ()
        {
            engineBehaviour.OnBehaviourUpdate -= SampleInput;
            if (gameObject) Object.Destroy(gameObject);
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var state = new GameState() {
                ProcessInput = ProcessInput
            };
            stateMap.SerializeObject(state);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            ProcessInput = state.ProcessInput;
            return Task.CompletedTask;
        }

        public InputSampler GetSampler (string bindingName)
        {
            if (!samplersMap.ContainsKey(bindingName)) return null;
            return samplersMap[bindingName];
        }

        /// <summary>
        /// Provided UI will block input processing of all the samplers, except <paramref name="allowedSamplers"/> when visible.
        /// </summary>
        public void AddBlockingUI (IManagedUI ui, params string[] allowedSamplers)
        {
            if (blockingUIs.ContainsKey(ui)) return;
            blockingUIs.Add(ui, allowedSamplers);
            ui.OnVisibilityChanged += HandleBlockingUIVisibilityChanged;
            HandleBlockingUIVisibilityChanged(ui.IsVisible);
        }

        /// <summary>
        /// Provided UI will no longer block input processing when visible.
        /// </summary>
        public void RemoveBlockingUI (IManagedUI ui)
        {
            if (!blockingUIs.ContainsKey(ui)) return;
            blockingUIs.Remove(ui);
            ui.OnVisibilityChanged -= HandleBlockingUIVisibilityChanged;
            HandleBlockingUIVisibilityChanged(ui.IsVisible);
        }

        private void HandleBlockingUIVisibilityChanged (bool isVisible)
        {
            // If any of the blocking UIs are visible, all the samplers should be blocked,
            // except ones that are explicitly allowed by ALL the visible blocking UIs.
            
            // 1. Find the allowed samplers first; start with clearing the set.
            blockedSamplers.Clear();
            // 2. Store all the existing samplers.
            blockedSamplers.UnionWith(samplersMap.Keys);
            // 3. Remove samplers that are not allowed by any of the visible blocking UIs.
            foreach (var kv in blockingUIs)
                if (kv.Key.IsVisible)
                    blockedSamplers.IntersectWith(kv.Value);
            // 4. This will filter-out the samplers contained in both collections,
            // effectively storing only the non-allowed (blocked) ones in the set.
            blockedSamplers.SymmetricExceptWith(samplersMap.Keys);
        }

        private void SampleInput ()
        {
            if (!ProcessInput) return;

            foreach (var kv in samplersMap)
                if (!blockedSamplers.Contains(kv.Key) || kv.Value.Binding.AlwaysProcess)
                    kv.Value.SampleInput();
        }
    } 
}
