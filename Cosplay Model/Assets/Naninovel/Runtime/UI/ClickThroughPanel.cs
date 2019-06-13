// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine.EventSystems;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a full-screen invisible UI panel, which blocks UI interaction and all (or a subset of) the input samplers while visible,
    /// but will hide itself and execute (if registered) `onClickThrough` callback when user clicks the panel.
    /// </summary>
    public class ClickThroughPanel : ScriptableUIBehaviour, IManagedUI, IPointerClickHandler
    {
        private Action onClickThrough;
        private InputManager inputManager;

        public Task InitializeAsync () => Task.CompletedTask;

        public void Show (Action onClickThrough, params string[] allowedSamplers)
        {
            this.onClickThrough = onClickThrough;
            Show();
            inputManager.AddBlockingUI(this, allowedSamplers);
        }

        public override void Hide ()
        {
            inputManager.RemoveBlockingUI(this);
            base.Hide();
        }

        public void OnPointerClick (PointerEventData eventData)
        {
            Hide();
            onClickThrough?.Invoke();
            onClickThrough = null;
        }

        protected override void Awake ()
        {
            base.Awake();
            inputManager = Engine.GetService<InputManager>();
        }
    }
}
