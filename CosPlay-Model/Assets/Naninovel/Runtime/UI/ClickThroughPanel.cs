// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine.EventSystems;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a full-screen invisible UI panel, which blocks UI interaction and all (or a subset of) the input samplers while visible,
    /// but can hide itself and execute (if registered) `onClick` callback when user clicks the panel.
    /// </summary>
    public class ClickThroughPanel : ScriptableUIBehaviour, IManagedUI, IPointerClickHandler
    {
        private Action onClick;
        private InputManager inputManager;
        private bool hideOnClick;

        public Task InitializeAsync () => Task.CompletedTask;

        public void Show (bool hideOnClick, Action onClick, params string[] allowedSamplers)
        {
            this.hideOnClick = hideOnClick;
            this.onClick = onClick;
            Show();
            inputManager.AddBlockingUI(this, allowedSamplers);
        }

        public override void Hide ()
        {
            onClick = null;
            inputManager.RemoveBlockingUI(this);
            base.Hide();
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            onClick?.Invoke();
            if (hideOnClick) Hide();
        }

        protected override void Awake ()
        {
            base.Awake();
            inputManager = Engine.GetService<InputManager>();
        }
    }
}
