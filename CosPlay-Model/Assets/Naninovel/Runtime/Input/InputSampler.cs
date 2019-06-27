// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Naninovel
{
    public class InputSampler
    {
        /// <summary>
        /// Invoked when input activation started.
        /// </summary>
        public event Action OnStart;
        /// <summary>
        /// Invoked when input activation ended.
        /// </summary>
        public event Action OnEnd;

        /// <summary>
        /// Assigned input binding.
        /// </summary>
        public InputBinding Binding { get; }
        /// <summary>
        /// Whether input is being activated.
        /// </summary>
        public bool IsActive { get; private set; }
        /// <summary>
        /// Whether input started activation during current frame.
        /// </summary>
        public bool StartedDuringFrame => IsActive && Time.frameCount == lastActiveFrame;
        /// <summary>
        /// Whether input ended activation during current frame.
        /// </summary>
        public bool EndedDuringFrame => !IsActive && Time.frameCount == lastActiveFrame;

        private HashSet<GameObject> objectTriggers;
        private TaskCompletionSource<bool> onInputTCS;
        private TaskCompletionSource<object> onInputStartTCS, onInputEndTCS;
        private CancellationTokenSource onInputStartCTS, onInputEndCTS;
        private float touchCooldown, lastTouchTime;
        private int lastActiveFrame;

        /// <param name="binding">Binding to trigger input.</param>
        /// <param name="objectTriggers">Objects to trigger input.</param>
        /// <param name="touchCooldown">Delay for detecting touch input state changes.</param>
        public InputSampler (InputBinding binding, IEnumerable<GameObject> objectTriggers = null, float touchCooldown = .1f)
        {
            Binding = binding;
            this.objectTriggers = objectTriggers != null ? new HashSet<GameObject>(objectTriggers) : new HashSet<GameObject>();
            this.touchCooldown = touchCooldown;
        }

        /// <summary>
        /// When any of the provided game objects are clicked or touched, input event will trigger.
        /// </summary>
        public void AddObjectTrigger (GameObject obj) => objectTriggers.Add(obj);

        public void RemoveObjectTrigger (GameObject obj) => objectTriggers.Remove(obj);

        /// <summary>
        /// Waits until input starts or ends activation.
        /// </summary>
        /// <returns>Whether input started or ended activation.</returns>
        public async Task<bool> WaitForInputAsync ()
        {
            if (onInputTCS is null) onInputTCS = new TaskCompletionSource<bool>();
            return await onInputTCS.Task;
        }

        /// <summary>
        /// Waits until input starts activation.
        /// </summary>
        public async Task WaitForInputStartAsync ()
        {
            if (onInputStartTCS is null) onInputStartTCS = new TaskCompletionSource<object>();
            await onInputStartTCS.Task;
        }

        /// <summary>
        /// Waits until input ends activation.
        /// </summary>
        public async Task WaitForInputEndAsync ()
        {
            if (onInputEndTCS is null) onInputEndTCS = new TaskCompletionSource<object>();
            await onInputEndTCS.Task;
        }

        /// <summary>
        /// Returned token will be canceled on next input start activation.
        /// </summary>
        public CancellationToken GetInputStartCancellationToken ()
        {
            if (onInputStartCTS is null) onInputStartCTS = new CancellationTokenSource();
            return onInputStartCTS.Token;
        }

        /// <summary>
        /// Returned token will be canceled on next input end activation.
        /// </summary>
        public CancellationToken GetInputEndCancellationToken ()
        {
            if (onInputEndCTS is null) onInputEndCTS = new CancellationTokenSource();
            return onInputEndCTS.Token;
        }

        public void SampleInput ()
        {
            if (Binding.Keys != null)
                foreach (var key in Binding.Keys)
                {
                    if (Input.GetKeyDown(key)) SetInputActive(true);
                    if (Input.GetKeyUp(key)) SetInputActive(false);
                }

            if (Binding.Axes != null)
                foreach (var axis in Binding.Axes)
                {
                    var axisSample = axis.Sample();
                    if (!axisSample.HasValue) continue;
                    if (axisSample.Value) SetInputActive(true);
                    else SetInputActive(false);
                }

            if (objectTriggers.Count > 0)
            {
                var touchBegan = Input.touchCount > 0
                    && Input.GetTouch(0).phase == TouchPhase.Began
                    && (Time.time - lastTouchTime) > touchCooldown;
                if (touchBegan) lastTouchTime = Time.time;
                var clickedDown = Input.GetMouseButtonDown(0);
                if (clickedDown || touchBegan)
                {
                    var hoveredObject = EventSystem.current.GetHoveredGameObject();
                    if (hoveredObject && objectTriggers.Contains(hoveredObject))
                        SetInputActive(true);
                }

                var touchEnded = Input.touchCount > 0
                    && Input.GetTouch(0).phase == TouchPhase.Ended;
                var clickedUp = Input.GetMouseButtonUp(0);
                if (touchEnded || clickedUp) SetInputActive(false);
            }
        }

        private void SetInputActive (bool isActive)
        {
            IsActive = isActive;
            lastActiveFrame = Time.frameCount;

            onInputTCS?.TrySetResult(isActive);
            onInputTCS = null;
            if (isActive)
            {
                onInputStartTCS?.TrySetResult(null);
                onInputStartTCS = null;
                onInputStartCTS?.Cancel();
                onInputStartCTS?.Dispose();
                onInputStartCTS = null;
            }
            else
            {
                onInputEndTCS?.TrySetResult(null);
                onInputEndTCS = null;
                onInputEndCTS?.Cancel();
                onInputEndCTS?.Dispose();
                onInputEndCTS = null;
            }
           
            if (isActive) OnStart?.Invoke();
            else OnEnd?.Invoke();
        }
    }
}
