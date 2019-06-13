// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class NavigatorSyncButton : ScriptableButton
    {
        private Image syncImage;
        private NovelScriptManager scriptManager;

        protected override void Awake ()
        {
            base.Awake();

            syncImage = GetComponentInChildren<Image>();
            this.AssertRequiredObjects(syncImage);

            scriptManager = Engine.GetService<NovelScriptManager>();
            scriptManager.OnScriptLoadStarted += ControlInteractability;
            scriptManager.OnScriptLoadCompleted += ControlInteractability;
        }

        private void Update ()
        {
            if (scriptManager is null || !scriptManager.IsNavigatorVisible) return;

            if (scriptManager.IsLoadingScripts) syncImage.rectTransform.Rotate(new Vector3(0, 0, -99) * Time.unscaledDeltaTime);
            else syncImage.rectTransform.rotation = Quaternion.identity;
        }

        protected override void OnButtonClick ()
        {
            scriptManager.ReloadAllScriptsAsync().WrapAsync();
        }

        private void ControlInteractability ()
        {
            UIComponent.interactable = !scriptManager.IsLoadingScripts;
        }
    } 
}
