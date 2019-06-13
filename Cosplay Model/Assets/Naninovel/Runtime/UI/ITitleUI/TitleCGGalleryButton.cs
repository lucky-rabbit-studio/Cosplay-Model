// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TitleCGGalleryButton : ScriptableButton
    {
        [ManagedText("UITitleMenu")]
        public readonly static string LabelText = "CG GALLERY";

        private UIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            uiManager = Engine.GetService<UIManager>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void Start ()
        {
            base.Start();

            var galleryUI = uiManager.GetUI<ICGGalleryUI>();
            if (galleryUI is null || galleryUI.CGCount == 0)
                gameObject.SetActive(false);
        }

        protected override void OnButtonClick () => uiManager.GetUI<ICGGalleryUI>()?.Show();
    }
}
