// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Handles <see cref="Script"/> execution (playback).
    /// </summary>
    [InitializeAtRuntime]
    public class ScriptPlayer : IStatefulService<SettingsStateMap>, IStatefulService<GlobalStateMap>, IStatefulService<GameStateMap>
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

        private readonly struct ExecutionSnapshot
        {
            public readonly string PlayedScriptName;
            public readonly int PlayedIndex;

            public ExecutionSnapshot (string playedScriptName, int playedIndex)
            {
                PlayedScriptName = playedScriptName;
                PlayedIndex = playedIndex;
            }
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
        /// Event invoked when player starts executing a <see cref="Command"/>.
        /// </summary>
        public event Action<Command> OnCommandExecutionStart;
        /// <summary>
        /// Event invoked when player finishes executing a <see cref="Command"/>.
        /// </summary>
        public event Action<Command> OnCommandExecutionFinish;
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
        /// Checks whether a follow-up command after the currently played one exists.
        /// </summary>
        public bool IsNextCommandAvailable => Playlist?.IsIndexValid(PlayedIndex + 1) ?? false;
        /// <summary>
        /// Whether skip mode is currently active.
        /// </summary>
        public bool IsSkipActive { get; private set; }
        /// <summary>
        /// Whether auto play mode is currently active.
        /// </summary>
        public bool IsAutoPlayActive { get; private set; }
        /// <summary>
        /// Whether user input is required to execute next script command.
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
        public Script PlayedScript { get; private set; }
        /// <summary>
        /// Currently played command.
        /// </summary>
        public Command PlayedCommand => Playlist?.GetCommandByIndex(PlayedIndex);
        /// <summary>
        /// List of <see cref="Command"/> built upon the currently played <see cref="Script"/>.
        /// </summary>
        public ScriptPlaylist Playlist { get; private set; }
        /// <summary>
        /// Index of the currently played command inside the <see cref="Playlist"/>.
        /// </summary>
        public int PlayedIndex { get; private set; }
        /// <summary>
        /// Last playback return spots stack registered by <see cref="Gosub"/> commands.
        /// </summary>
        public Stack<PlaybackSpot> LastGosubReturnSpots { get; private set; }
        /// <summary>
        /// Total number of commands existing in all the available naninovel scripts.
        /// </summary>
        public int TotalCommandsCount { get; private set; }
        /// <summary>
        /// Total number of unique commands ever played by the player (global state scope).
        /// </summary>
        public int PlayedCommandsCount => playedScriptRegister.CountPlayed();

        private bool PlayedCommandExecuted => executionStack.Count > 0 && executionStack.Peek().PlayedIndex == PlayedIndex;

        private readonly ScriptPlayerConfiguration config;
        private readonly InputManager inputManager;
        private readonly ScriptManager scriptManager;
        private readonly ResourceProviderManager providerManager;
        private readonly Stack<ExecutionSnapshot> executionStack = new Stack<ExecutionSnapshot>();
        private PlayedScriptRegister playedScriptRegister;
        private CancellationTokenSource playRoutineCTS;
        private TaskCompletionSource<object> waitForWaitForInputDisabledTCS;

        public ScriptPlayer (ScriptPlayerConfiguration config, ScriptManager scriptManager, 
            InputManager inputManager, ResourceProviderManager providerManager)
        {
            this.config = config;
            this.scriptManager = scriptManager;
            this.inputManager = inputManager;
            this.providerManager = providerManager;

            LastGosubReturnSpots = new Stack<PlaybackSpot>();
            playedScriptRegister = new PlayedScriptRegister();
        }

        public async Task InitializeServiceAsync ()
        {
            inputManager.Continue.OnStart += DisableWaitingForInput;
            inputManager.Skip.OnStart += EnableSkip;
            inputManager.Skip.OnEnd += DisableSkip;
            inputManager.AutoPlay.OnStart += ToggleAutoPlay;

            if (config.UpdateActionCountOnInit)
                TotalCommandsCount = await UpdateTotalActionCountAsync();
        }

        public void ResetService ()
        {
            Stop();
            executionStack.Clear();
            Playlist?.ReleaseResources();
            Playlist = null;
            PlayedIndex = -1;
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
                PlayedIndex = PlayedIndex,
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

            PlayedIndex = state.PlayedIndex;
            SetWaitingForInputActive(state.IsWaitingForInput);
            SkipNextWaitForInput = state.SkipNextWaitForInput;
            PlayedScript = await scriptManager.LoadScriptAsync(state.PlayedScriptName);
            LastGosubReturnSpots = new Stack<PlaybackSpot>(state.LastGosubReturnSpots);

            Playlist = new ScriptPlaylist(PlayedScript);

            var endIndex = providerManager.ResourcePolicy == ResourcePolicy.Static ? Playlist.Count - 1 : 
                Mathf.Min(PlayedIndex + providerManager.DynamicPolicySteps, Playlist.Count - 1);
            await Playlist.HoldResourcesAsync(PlayedIndex, endIndex);
        }

        public async Task<int> UpdateTotalActionCountAsync ()
        {
            TotalCommandsCount = 0;

            var scripts = await scriptManager.LoadAllScriptsAsync();
            foreach (var script in scripts)
            {
                var playlist = new ScriptPlaylist(script);
                TotalCommandsCount += playlist.Count;
            }

            return TotalCommandsCount;
        }

        /// <summary>
        /// Starts <see cref="PlayedScript"/> playback at <see cref="PlayedIndex"/>.
        /// </summary>
        public void Play () => Play(PlayedIndex);

        /// <summary>
        /// Starts <see cref="PlayedScript"/> playback at <paramref name="playlistIndex"/>.
        /// </summary>
        public void Play (int playlistIndex)
        {
            if (PlayedScript is null || Playlist is null)
            {
                Debug.LogError("Failed to start script playback: the script is not set.");
                return;
            }

            if (IsPlaying) Stop();

            PlayedIndex = playlistIndex;
            if (Playlist.IsIndexValid(PlayedIndex) || SelectNextCommand())
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
        /// <param name="startInlineIndex">Command inline index to start playback from.</param>
        public void Play (Script script, int startLineIndex = 0, int startInlineIndex = 0)
        {
            PlayedScript = script;

            if (Playlist is null || Playlist.ScriptName != script.Name)
                Playlist = new ScriptPlaylist(script);

            if (startLineIndex > 0 || startInlineIndex > 0)
            {
                var startAction = Playlist.GetFirstCommandAfterLine(startLineIndex, startInlineIndex);
                if (startAction is null) { Debug.LogError($"Script player failed to start: no commands found in script `{PlayedScript.Name}` at line #{startLineIndex}.{startInlineIndex}."); return; }
                PlayedIndex = Playlist.IndexOf(startAction);
            }
            else PlayedIndex = 0;

            Play();
        }

        /// <summary>
        /// Starts playback of the provided script at the provided label.
        /// </summary>
        /// <param name="script">The script to play.</param>
        /// <param name="label">Name of the label within provided script to start playback from.</param>
        public void Play (Script script, string label)
        {
            if (!script.LabelExists(label))
            {
                Debug.LogError($"Failed to jump to `{label}` label: label not found in `{script.Name}` script.");
                return;
            }

            Play(script, script.GetLineIndexForLabel(label));
        }

        /// <summary>
        /// Preloads the script's commands and starts playing.
        /// </summary>
        public async Task PreloadAndPlayAsync (Script script, int startLineIndex = 0, int startInlineIndex = 0)
        {
            Playlist = new ScriptPlaylist(script);
            var startAction = Playlist.GetFirstCommandAfterLine(startLineIndex, startInlineIndex);
            var startIndex = startAction != null ? Playlist.IndexOf(startAction) : 0;

            var endIndex = providerManager.ResourcePolicy == ResourcePolicy.Static ? Playlist.Count - 1 :
                Mathf.Min(startIndex + providerManager.DynamicPolicySteps, Playlist.Count - 1);
            await Playlist.HoldResourcesAsync(startIndex, endIndex);

            Play(script, startLineIndex, startInlineIndex);
        }

        /// <summary>
        /// Loads a script with the provided name, preloads the script's commands and starts playing.
        /// </summary>
        public async Task PreloadAndPlayAsync (string scriptName, int startLineIndex = 0, int startInlineIndex = 0, string label = null)
        {
            var script = await scriptManager.LoadScriptAsync(scriptName);
            if (script is null)
            {
                Debug.LogError($"Script player failed to start: script with name `{scriptName}` wasn't able to load.");
                return;
            }

            if (!string.IsNullOrEmpty(label))
            {
                if (!script.LabelExists(label))
                {
                    Debug.LogError($"Failed to jump to `{label}` label: label not found in `{script.Name}` script.");
                    return;
                }
                startLineIndex = script.GetLineIndexForLabel(label);
                startInlineIndex = 0;
            }

            await PreloadAndPlayAsync(script, startLineIndex, startInlineIndex);
        }

        /// <summary>
        /// Attempts to select next command in the current playlist.
        /// </summary>
        public async Task SelectNextAsync ()
        {
            if (Playlist is null || PlayedCommand is null) return;
            var nextCommand = Playlist.GetCommandByIndex(PlayedIndex + 1);
            if (nextCommand is null) return;
            await RewindAsync(nextCommand.LineIndex, nextCommand.InlineIndex);
        }

        /// <summary>
        /// Attempts to select previous command in the current playlist.
        /// </summary>
        public async Task SelectPreviousAsync ()
        {
            if (Playlist is null || PlayedCommand is null) return;
            var prevCommand = Playlist.GetCommandByIndex(PlayedIndex - 1);
            if (prevCommand is null) return;
            await RewindAsync(prevCommand.LineIndex, prevCommand.InlineIndex);
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
        /// Depending on the provided <paramref name="lineIndex"/> being before or after currently played command' line index,
        /// performs a fast-forward or fast-backward playback of the currently loaded script.
        /// </summary>
        /// <param name="lineIndex">The line index to rewind at.</param>
        /// <param name="inlineIndex">The inline index to rewind at.</param>
        /// <returns>Whether the <paramref name="lineIndex"/> has been reached.</returns>
        public async Task<bool> RewindAsync (int lineIndex, int inlineIndex = 0)
        {
            if (IsPlaying) Stop();

            if (PlayedCommand is null)
            {
                Debug.LogError("Script player failed to rewind: played command is not valid.");
                return false;
            }

            var targetAction = Playlist.GetFirstCommandAfterLine(lineIndex, inlineIndex);
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
            var targetIndex = Playlist.IndexOf(targetAction);
            var result = targetIndex > PlayedIndex ? await FastForwardRoutineAsync(token, lineIndex, inlineIndex) : await FastBackwardRoutineAsync(token, lineIndex, inlineIndex);

            Stop();

            return result;
        }

        /// <summary>
        /// Checks whether <see cref="IsSkipActive"/> can be enabled at the moment.
        /// Result depends on <see cref="PlayerSkipMode"/> and currently played command.
        /// </summary>
        public bool IsSkipAllowed ()
        {
            if (SkipMode == PlayerSkipMode.Everything) return true;
            if (PlayedScript is null) return false;
            return playedScriptRegister.IsIndexPlayed(PlayedScript.Name, PlayedIndex);
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

        private async Task WaitForWaitForInputDisabledAsync ()
        {
            if (waitForWaitForInputDisabledTCS is null)
                waitForWaitForInputDisabledTCS = new TaskCompletionSource<object>();
            await waitForWaitForInputDisabledTCS.Task;
        }

        private async Task WaitForAutoPlayDelayAsync ()
        {
            var printer = Engine.GetService<TextPrinterManager>()?.GetActivePrinter();
            var delay = printer is null ? 0 : printer.PrintDelay * Regex.Replace(printer.LastPrintedText ?? "", "(<.*?>)|(\\[.*?\\])", string.Empty).Length;
            delay = Mathf.Clamp(delay, config.MinAutoPlayDelay, float.PositiveInfinity);
            await new WaitForSeconds(delay);
            if (!IsAutoPlayActive) await WaitForWaitForInputDisabledAsync(); // In case auto play was disabled while waiting for delay.
        }

        private async Task ExecutePlayedCommandAsync ()
        {
            if (PlayedCommand is null || !PlayedCommand.ShouldExecute) return;

            playedScriptRegister.RegisterPlayedIndex(PlayedScript.Name, PlayedIndex);
            executionStack.Push(new ExecutionSnapshot(PlayedScript.Name, PlayedIndex));

            OnCommandExecutionStart?.Invoke(PlayedCommand);

            if (PlayedCommand.Wait) await PlayedCommand.ExecuteAsync();
            else PlayedCommand.ExecuteAsync().WrapAsync();

            if (providerManager.ResourcePolicy == ResourcePolicy.Dynamic)
            {
                if (PlayedCommand is Command.IPreloadable playedPreloadableCmd)
                    playedPreloadableCmd.ReleaseResources();
                // TODO: Handle @goto, @if/else/elseif and all the conditionally executed actions. (just unload everything that has a lower play index?)
                if (Playlist.GetCommandByIndex(PlayedIndex + providerManager.DynamicPolicySteps) is Command.IPreloadable nextPreloadableCmd)
                    nextPreloadableCmd.HoldResourcesAsync().WrapAsync();
            }

            OnCommandExecutionFinish?.Invoke(PlayedCommand);
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

                await ExecutePlayedCommandAsync();

                if (cancellationToken.IsCancellationRequested) break;

                var nextActionAvailable = SelectNextCommand();
                if (!nextActionAvailable) break;

                if (IsSkipActive && !IsSkipAllowed()) SetSkipActive(false);
            }
        }

        private async Task<bool> FastForwardRoutineAsync (CancellationToken cancellationToken, int lineIndex, int inlineIndex)
        {
            SetSkipActive(true);
            if (!PlayedCommandExecuted) await ExecutePlayedCommandAsync();
            if (cancellationToken.IsCancellationRequested) { SetSkipActive(false); return false; }

            var reachedLine = true;
            while (Engine.IsInitialized && IsPlaying)
            {
                var nextActionAvailable = SelectNextCommand();
                if (!nextActionAvailable) { reachedLine = false; break; }

                if (PlayedCommand.LineIndex > lineIndex) { reachedLine = true; break; }
                if (PlayedCommand.LineIndex == lineIndex && PlayedCommand.InlineIndex >= inlineIndex) { reachedLine = true; break; }

                SetSkipActive(true);
                await ExecutePlayedCommandAsync();

                if (cancellationToken.IsCancellationRequested) { reachedLine = false; break; }
            }
            SetSkipActive(false);
            return reachedLine;
        }

        private async Task<bool> FastBackwardRoutineAsync (CancellationToken cancellationToken, int lineIndex, int inlineIndex)
        {
            if (PlayedCommandExecuted) await PlayedCommand.UndoAsync();
            if (cancellationToken.IsCancellationRequested) return false;

            while (Engine.IsInitialized && IsPlaying)
            {
                var previousActionAvailable = SelectPreviouslyExecutedCommand();
                if (!previousActionAvailable) return false;

                await PlayedCommand.UndoAsync();

                if (PlayedCommand.LineIndex < lineIndex) return true;
                if (PlayedCommand.LineIndex == lineIndex && PlayedCommand.InlineIndex <= inlineIndex) return true;

                if (cancellationToken.IsCancellationRequested) return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to select next <see cref="Command"/> in the current <see cref="Playlist"/>.
        /// </summary>
        /// <returns>Whether next command is available and was selected.</returns>
        private bool SelectNextCommand ()
        {
            PlayedIndex++;
            if (Playlist.IsIndexValid(PlayedIndex)) return true;

            // No commands left in the played script.
            Debug.Log($"Script '{PlayedScript.Name}' has finished playing, and there wasn't a follow-up goto command. " +
                        "Consider using stop command in case you wish to gracefully stop script execution.");
            return false;
        }

        /// <summary>
        /// Attempts to select previously executed <see cref="Command"/> in the current <see cref="Playlist"/>.
        /// </summary>
        /// <returns>Whether previous command is available and was selected.</returns>
        private bool SelectPreviouslyExecutedCommand ()
        {
            if (PlayedScript is null || Playlist is null || executionStack.Count == 0) return false;

            var previous = executionStack.Pop();
            if (previous.PlayedScriptName != PlayedScript.Name) return false;
            if (!Playlist.IsIndexValid(previous.PlayedIndex)) return false;

            PlayedIndex = previous.PlayedIndex;

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
