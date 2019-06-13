// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Handles <see cref="NovelScript"/> execution.
    /// </summary>
    [InitializeAtRuntime]
    public class NovelScriptPlayer : IStatefulService<SettingsStateMap>, IStatefulService<GlobalStateMap>, IStatefulService<GameStateMap>
    {
        [Serializable]
        private class Settings
        {
            public PlayerSkipMode SkipMode = PlayerSkipMode.ReadOnly;
        }

        [Serializable]
        private class GlobalState
        {
            public PlayedScriptRegister PlayedScriptRegister = new PlayedScriptRegister();
        }

        [Serializable]
        private class GameState
        {
            public string PlayedScriptName;
            public int PlayedIndex;
            public bool IsWaitingForInput, SkipNextWaitForInput;
            public List<PlaybackSpot> LastGosubReturnSpots = new List<PlaybackSpot>();
        }

        private struct ExecutionSnapshot
        {
            public string PlayedScriptName;
            public int PlayedIndex;
        }

        /// <summary>
        /// Event invoked when player starts playing a script.
        /// </summary>
        public event Action OnPlay;
        /// <summary>
        /// Event invoked when player stops playing a script.
        /// </summary>
        public event Action OnStop;
        /// <summary>
        /// Event invoked when player starts executing a <see cref="NovelAction"/>.
        /// </summary>
        public event Action<NovelAction> OnActionExecutionStart;
        /// <summary>
        /// Event invoked when player finishes executing a <see cref="NovelAction"/>.
        /// </summary>
        public event Action<NovelAction> OnActionExecutionFinish;
        /// <summary>
        /// Event invoked when skip mode changes.
        /// </summary>
        public event Action<bool> OnSkip;
        /// <summary>
        /// Event invoked when auto play mode changes.
        /// </summary>
        public event Action<bool> OnAutoPlay;
        /// <summary>
        /// Event invoked when waiting for input mode changes.
        /// </summary>
        public event Action<bool> OnWaitingForInput;

        /// <summary>
        /// Whether the player is currently playing a script.
        /// </summary>
        public bool IsPlaying => playRoutineCTS != null;
        /// <summary>
        /// Checks whether a follow-up action after the currently played one exists.
        /// </summary>
        public bool IsNextActionAvailable => playlist?.IsIndexValid(playedIndex + 1) ?? false;
        /// <summary>
        /// Whether skip mode is currently active.
        /// </summary>
        public bool IsSkipActive { get; private set; }
        /// <summary>
        /// Whether auto play mode is currently active.
        /// </summary>
        public bool IsAutoPlayActive { get; private set; }
        /// <summary>
        /// Whether user input is required to execute next script action.
        /// </summary>
        public bool IsWaitingForInput { get; private set; }
        /// <summary>
        /// Whether to ignore next <see cref="EnableWaitingForInput"/>.
        /// </summary>
        public bool SkipNextWaitForInput { get; set; }
        /// <summary>
        /// Skip mode to use while <see cref="IsSkipActive"/>.
        /// </summary>
        public PlayerSkipMode SkipMode { get; set; }
        /// <summary>
        /// Currently played script.
        /// </summary>
        public NovelScript PlayedScript { get; private set; }
        /// <summary>
        /// Currently played action.
        /// </summary>
        public NovelAction PlayedAction => playlist?.GetActionByIndex(playedIndex);
        /// <summary>
        /// Last playback return spots stack registered by <see cref="Gosub"/> actions.
        /// </summary>
        public Stack<PlaybackSpot> LastGosubReturnSpots { get; private set; }
        /// <summary>
        /// Total number of actions existing in all the available novel scripts.
        /// </summary>
        public int TotalActionCount { get; private set; }
        /// <summary>
        /// Total number of unique actions ever played by the player (global state scope).
        /// </summary>
        public int PlayedActionCount => playedScriptRegister.CountPlayed();

        private readonly ScriptPlayerConfiguration config;
        private InputManager inputManager;
        private NovelScriptManager scriptManager;
        private NovelScriptPlaylist playlist;
        private int playedIndex;
        private CancellationTokenSource playRoutineCTS;
        private TaskCompletionSource<object> waitForWaitForInputDisabledTCS;
        private Stack<ExecutionSnapshot> executionStack;
        private PlayedScriptRegister playedScriptRegister;

        public NovelScriptPlayer (ScriptPlayerConfiguration config, NovelScriptManager scriptManager, InputManager inputManager)
        {
            this.config = config;
            this.scriptManager = scriptManager;
            this.inputManager = inputManager;

            executionStack = new Stack<ExecutionSnapshot>();
            playedScriptRegister = new PlayedScriptRegister();
            LastGosubReturnSpots = new Stack<PlaybackSpot>();
        }

        public async Task InitializeServiceAsync ()
        {
            inputManager.Continue.OnStart += DisableWaitingForInput;
            inputManager.Skip.OnStart += EnableSkip;
            inputManager.Skip.OnEnd += DisableSkip;
            inputManager.AutoPlay.OnStart += ToggleAutoPlay;

            if (config.UpdateActionCountOnInit)
                TotalActionCount = await UpdateTotalActionCountAsync();
        }

        public void ResetService ()
        {
            Stop();
            executionStack.Clear();
            playlist?.UnloadActionsAsync().WrapAsync();
            playlist = null;
            playedIndex = -1;
            PlayedScript = null;
            DisableWaitingForInput();
            DisableAutoPlay();
            DisableSkip();
        }

        public void DestroyService ()
        {
            Stop();
            inputManager.Continue.OnStart -= DisableWaitingForInput;
            inputManager.Skip.OnStart -= EnableSkip;
            inputManager.Skip.OnEnd -= DisableSkip;
            inputManager.AutoPlay.OnStart -= ToggleAutoPlay;
        }

        public Task SaveServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                SkipMode = SkipMode
            };
            stateMap.SerializeObject(settings);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.DeserializeObject<Settings>() ?? new Settings();
            SkipMode = settings.SkipMode;
            return Task.CompletedTask;
        }

        public Task SaveServiceStateAsync (GlobalStateMap stateMap)
        {
            var globalState = new GlobalState {
                PlayedScriptRegister = playedScriptRegister
            };
            stateMap.SerializeObject(globalState);
            return Task.CompletedTask;
        }

        public Task LoadServiceStateAsync (GlobalStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GlobalState>() ?? new GlobalState();
            playedScriptRegister = state.PlayedScriptRegister;
            return Task.CompletedTask;
        }

        public Task SaveServiceStateAsync (GameStateMap stateMap)
        {
            var gameState = new GameState() {
                PlayedScriptName = PlayedScript?.Name,
                PlayedIndex = playedIndex,
                IsWaitingForInput = IsWaitingForInput,
                SkipNextWaitForInput = SkipNextWaitForInput,
                LastGosubReturnSpots = LastGosubReturnSpots.Reverse().ToList() // Stack is reversed on enum.
            };
            stateMap.SerializeObject(gameState);
            return Task.CompletedTask;
        }

        public async Task LoadServiceStateAsync (GameStateMap stateMap)
        {
            var state = stateMap.DeserializeObject<GameState>() ?? new GameState();
            if (string.IsNullOrEmpty(state.PlayedScriptName)) return;

            playedIndex = state.PlayedIndex;
            SetWaitingForInputActive(state.IsWaitingForInput);
            SkipNextWaitForInput = state.SkipNextWaitForInput;
            PlayedScript = await scriptManager.LoadScriptAsync(state.PlayedScriptName);
            LastGosubReturnSpots = new Stack<PlaybackSpot>(state.LastGosubReturnSpots);

            playlist = new NovelScriptPlaylist(PlayedScript);
            await playlist.PreloadActionsAsync(playedIndex);
        }

        public async Task<int> UpdateTotalActionCountAsync ()
        {
            TotalActionCount = 0;

            var scripts = await scriptManager.LoadAllScriptsAsync();
            foreach (var script in scripts)
            {
                var playlist = new NovelScriptPlaylist(script);
                TotalActionCount += playlist.Count;
            }

            return TotalActionCount;
        }

        /// <summary>
        /// Starts <see cref="PlayedScript"/> playback at <see cref="playedIndex"/>.
        /// </summary>
        public void Play ()
        {
            if (PlayedScript is null || playlist is null) { Debug.LogError("Failed to start script playback: the script is not set."); return; }

            if (IsPlaying) Stop();

            if (playlist.IsIndexValid(playedIndex) || SelectNextAction())
            {
                playRoutineCTS = new CancellationTokenSource();
                PlayRoutineAsync(playRoutineCTS.Token).WrapAsync();
                OnPlay?.Invoke();
            }
        }

        /// <summary>
        /// Starts playback of the provided script at the provided line and inline indexes.
        /// </summary>
        /// <param name="script">The script to play.</param>
        /// <param name="startLineIndex">Line index to start playback from.</param>
        /// <param name="startInlineIndex">Action inline index to start playback from.</param>
        public void Play (NovelScript script, int startLineIndex = 0, int startInlineIndex = 0)
        {
            PlayedScript = script;

            if (playlist is null || playlist.ScriptName != script.Name)
                playlist = new NovelScriptPlaylist(script);

            if (startLineIndex > 0 || startInlineIndex > 0)
            {
                var startAction = playlist.GetFirstActionAfterLine(startLineIndex, startInlineIndex);
                if (startAction is null) { Debug.LogError($"Script player failed to start: no actions found in script `{PlayedScript.Name}` at line #{startLineIndex}.{startInlineIndex}."); return; }
                playedIndex = playlist.IndexOf(startAction);
            }
            else playedIndex = 0;

            Play();
        }

        /// <summary>
        /// Starts playback of the provided script at the provided label.
        /// </summary>
        /// <param name="script">The script to play.</param>
        /// <param name="label">Name of the label within provided script to start playback from.</param>
        public void Play (NovelScript script, string label)
        {
            if (!script.LabelExists(label))
            {
                Debug.LogError($"Failed to jump to `{label}` label: label not found in `{script.Name}` script.");
                return;
            }

            Play(script, script.GetLineIndexForLabel(label));
        }

        /// <summary>
        /// Preloads the script's actions and starts playing.
        /// </summary>
        public async Task PreloadAndPlayAsync (NovelScript script, int startLineIndex = 0, int startInlineIndex = 0, string label = null)
        {
            playlist = new NovelScriptPlaylist(script);
            var startAction = playlist.GetFirstActionAfterLine(startLineIndex, startInlineIndex);
            var startIndex = startAction != null ? playlist.IndexOf(startAction) : 0;
            await playlist.PreloadActionsAsync(startIndex);
            if (string.IsNullOrEmpty(label)) Play(script, startLineIndex, startInlineIndex);
            else Play(script, label);
        }

        /// <summary>
        /// Loads a script with the provided name, preloads the script's actions and starts playing.
        /// </summary>
        public async Task PreloadAndPlayAsync (string scriptName, int startLineIndex = 0, int startInlineIndex = 0, string label = null)
        {
            var script = await scriptManager.LoadScriptAsync(scriptName);
            if (script is null) { Debug.LogError($"Script player failed to start: script with name `{scriptName}` wasn't able to load."); return; }
            await PreloadAndPlayAsync(script, startLineIndex, startInlineIndex, label);
        }

        /// <summary>
        /// Attempts to select next action in the current playlist.
        /// </summary>
        public async Task SelectNext ()
        {
            if (playlist is null || PlayedAction is null) return;
            var nextAction = playlist.GetActionByIndex(playedIndex + 1);
            if (nextAction is null) return;
            await RewindAsync(nextAction.LineIndex, nextAction.InlineIndex);
        }

        /// <summary>
        /// Attempts to select previous action in the current playlist.
        /// </summary>
        public async Task SelectPrevious ()
        {
            if (playlist is null || PlayedAction is null) return;
            var prevAction = playlist.GetActionByIndex(playedIndex - 1);
            if (prevAction is null) return;
            await RewindAsync(prevAction.LineIndex, prevAction.InlineIndex);
        }

        /// <summary>
        /// Halts the playback of the currently played script.
        /// </summary>
        public void Stop ()
        {
            if (!IsPlaying) return;

            playRoutineCTS.Cancel();
            playRoutineCTS.Dispose();
            playRoutineCTS = null;

            OnStop?.Invoke();
        }

        /// <summary>
        /// Depending on the provided <paramref name="lineIndex"/> being before or after currently played action' line index,
        /// performs a fast-forward or fast-backward playback of the currently loaded script.
        /// </summary>
        /// <param name="lineIndex">The line index to rewind at.</param>
        /// <param name="inlineIndex">The inline index to rewind at.</param>
        /// <returns>Whether the <paramref name="lineIndex"/> has been reached.</returns>
        public async Task<bool> RewindAsync (int lineIndex, int inlineIndex = 0)
        {
            if (IsPlaying) Stop();

            if (PlayedAction is null)
            {
                Debug.LogError("Script player failed to rewind: played action is not valid.");
                return false;
            }

            var targetAction = playlist.GetFirstActionAfterLine(lineIndex, inlineIndex);
            if (targetAction is null)
            {
                Debug.LogError($"Script player failed to rewind: target line index ({lineIndex}) is not valid for `{PlayedScript.Name}` script.");
                return false;
            }

            DisableAutoPlay();
            DisableSkip();
            DisableWaitingForInput();

            playRoutineCTS = new CancellationTokenSource();
            var token = playRoutineCTS.Token;
            var targetIndex = playlist.IndexOf(targetAction);
            var result = targetIndex > playedIndex ? await FastForwardRoutineAsync(token, lineIndex, inlineIndex) : await FastBackwardRoutineAsync(token, lineIndex, inlineIndex);

            Stop();

            return result;
        }

        /// <summary>
        /// Checks whether <see cref="IsSkipActive"/> can be enabled at the moment.
        /// Result depends on <see cref="PlayerSkipMode"/> and currently played action.
        /// </summary>
        public bool IsSkipAllowed ()
        {
            if (SkipMode == PlayerSkipMode.Everything) return true;
            if (PlayedScript is null) return false;
            return playedScriptRegister.IsIndexPlayed(PlayedScript.Name, playedIndex);
        }

        /// <summary>
        /// Enables <see cref="IsSkipActive"/> when <see cref="IsSkipAllowed"/>.
        /// </summary>
        public void EnableSkip ()
        {
            if (!IsSkipAllowed()) return;
            SetSkipActive(true);
        }

        /// <summary>
        /// Disables <see cref="IsSkipActive"/>.
        /// </summary>
        public void DisableSkip () => SetSkipActive(false);

        public void EnableAutoPlay () => SetAutoPlayActive(true);

        public void DisableAutoPlay () => SetAutoPlayActive(false);

        public void ToggleAutoPlay ()
        {
            if (IsAutoPlayActive) DisableAutoPlay();
            else EnableAutoPlay();
        }

        public void EnableWaitingForInput ()
        {
            if (SkipNextWaitForInput)
            {
                SkipNextWaitForInput = false;
                return;
            }

            if (IsSkipActive) return;
            SetWaitingForInputActive(true);
        }

        public void DisableWaitingForInput () => SetWaitingForInputActive(false);

        private bool PlayedActionExecuted ()
        {
            if (executionStack.Count == 0) return false;
            return executionStack.Peek().PlayedIndex == playedIndex;
        }

        private async Task WaitForWaitForInputDisabledAsync ()
        {
            if (waitForWaitForInputDisabledTCS is null)
                waitForWaitForInputDisabledTCS = new TaskCompletionSource<object>();
            await waitForWaitForInputDisabledTCS.Task;
        }

        private async Task WaitForAutoPlayDelayAsync ()
        {
            var printer = Engine.GetService<TextPrinterManager>()?.GetActivePrinter();
            var delay = printer is null ? 0 : printer.PrintDelay * System.Text.RegularExpressions.Regex.Replace(printer.LastPrintedText ?? "", "(<.*?>)|(\\[.*?\\])", string.Empty).Length;
            delay = Mathf.Clamp(delay, config.MinAutoPlayDelay, float.PositiveInfinity);
            await new WaitForSeconds(delay);
            if (!IsAutoPlayActive) await WaitForWaitForInputDisabledAsync(); // In case auto play was disabled while waiting for delay.
        }

        private async Task ExecutePlayedActionAsync ()
        {
            if (PlayedAction is null || !PlayedAction.ShouldExecute) return;

            playedScriptRegister.RegisterPlayedIndex(PlayedScript.Name, playedIndex);
            executionStack.Push(new ExecutionSnapshot { PlayedScriptName = PlayedScript.Name, PlayedIndex = playedIndex });

            OnActionExecutionStart?.Invoke(PlayedAction);

            if (PlayedAction.Wait) await PlayedAction.ExecuteAsync();
            else PlayedAction.ExecuteAsync().WrapAsync();

            OnActionExecutionFinish?.Invoke(PlayedAction);
        }

        private async Task PlayRoutineAsync (CancellationToken cancellationToken)
        {
            while (Engine.IsInitialized && IsPlaying)
            {
                if (IsWaitingForInput)
                {
                    if (IsAutoPlayActive) { await Task.WhenAny(WaitForAutoPlayDelayAsync(), WaitForWaitForInputDisabledAsync()); DisableWaitingForInput(); }
                    else await WaitForWaitForInputDisabledAsync();
                    if (cancellationToken.IsCancellationRequested) break;
                }

                await ExecutePlayedActionAsync();

                if (cancellationToken.IsCancellationRequested) break;

                var nextActionAvailable = SelectNextAction();
                if (!nextActionAvailable) break;

                if (IsSkipActive && !IsSkipAllowed()) SetSkipActive(false);
            }
        }

        private async Task<bool> FastForwardRoutineAsync (CancellationToken cancellationToken, int lineIndex, int inlineIndex)
        {
            SetSkipActive(true);
            if (!PlayedActionExecuted()) await ExecutePlayedActionAsync();
            if (cancellationToken.IsCancellationRequested) { SetSkipActive(false); return false; }

            var reachedLine = true;
            while (Engine.IsInitialized && IsPlaying)
            {
                var nextActionAvailable = SelectNextAction();
                if (!nextActionAvailable) { reachedLine = false; break; }

                if (PlayedAction.LineIndex > lineIndex) { reachedLine = true; break; }
                if (PlayedAction.LineIndex == lineIndex && PlayedAction.InlineIndex >= inlineIndex) { reachedLine = true; break; }

                SetSkipActive(true);
                await ExecutePlayedActionAsync();

                if (cancellationToken.IsCancellationRequested) { reachedLine = false; break; }
            }
            SetSkipActive(false);
            return reachedLine;
        }

        private async Task<bool> FastBackwardRoutineAsync (CancellationToken cancellationToken, int lineIndex, int inlineIndex)
        {
            if (PlayedActionExecuted()) await PlayedAction.UndoAsync();
            if (cancellationToken.IsCancellationRequested) return false;

            while (Engine.IsInitialized && IsPlaying)
            {
                var previousActionAvailable = SelectPreviouslyExecutedAction();
                if (!previousActionAvailable) return false;

                await PlayedAction.UndoAsync();

                if (PlayedAction.LineIndex < lineIndex) return true;
                if (PlayedAction.LineIndex == lineIndex && PlayedAction.InlineIndex <= inlineIndex) return true;

                if (cancellationToken.IsCancellationRequested) return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to select next <see cref="NovelAction"/> in the current <see cref="playlist"/>.
        /// </summary>
        /// <returns>Whether next action is available and was selected.</returns>
        private bool SelectNextAction ()
        {
            playedIndex++;
            if (playlist.IsIndexValid(playedIndex)) return true;

            // No actions left in the played script.
            Debug.Log($"Script '{PlayedScript.Name}' has finished playing, and there wasn't a follow-up goto action. " +
                        "Consider using stop command in case you wish to gracefully stop script execution.");
            return false;
        }

        /// <summary>
        /// Attempts to select previously executed <see cref="NovelAction"/> in the current <see cref="playlist"/>.
        /// </summary>
        /// <returns>Whether previous action is available and was selected.</returns>
        private bool SelectPreviouslyExecutedAction ()
        {
            if (PlayedScript is null || playlist is null || executionStack.Count == 0) return false;

            var previous = executionStack.Pop();
            if (previous.PlayedScriptName != PlayedScript.Name) return false;
            if (!playlist.IsIndexValid(previous.PlayedIndex)) return false;

            playedIndex = previous.PlayedIndex;

            return true;
        }

        private void SetSkipActive (bool isActive)
        {
            if (IsSkipActive == isActive) return;
            IsSkipActive = isActive;
            Time.timeScale = isActive ? config.SkipTimeScale : 1f;
            OnSkip?.Invoke(isActive);

            if (isActive && IsWaitingForInput) SetWaitingForInputActive(false);
        }

        private void SetAutoPlayActive (bool isActive)
        {
            if (IsAutoPlayActive == isActive) return;
            IsAutoPlayActive = isActive;
            OnAutoPlay?.Invoke(isActive);

            if (isActive && IsWaitingForInput) SetWaitingForInputActive(false);
        }

        private void SetWaitingForInputActive (bool isActive)
        {
            if (IsWaitingForInput == isActive) return;
            IsWaitingForInput = isActive;
            if (!isActive)
            {
                waitForWaitForInputDisabledTCS?.TrySetResult(null);
                waitForWaitForInputDisabledTCS = null;
            }
            OnWaitingForInput?.Invoke(isActive);
        }
    } 
}
