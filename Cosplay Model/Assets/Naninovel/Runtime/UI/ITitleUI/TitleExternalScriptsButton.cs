// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class TitleExternalScriptsButton : ScriptableButton
    {
        [ManagedText("UITitleMenu")]
        public readonly static string LabelText = "EXTERNAL SCRIPTS";

        private UIManager uiManager;
        private NovelScriptManager scriptManager;

        protected override void Awake ()
        {
            base.Awake();

            scriptManager = Engine.GetService<NovelScriptManager>();
            uiManager = Engine.GetService<UIManager>();
            UIComponent.GetComponentInChildren<Text>().text = LabelText;
        }

        protected override void Start ()
        {
            base.Start();

            if (!scriptManager.CommunityModdingEnabled)
                gameObject.SetActive(false);
        }

        protected override void OnButtonClick () => uiManager.GetUI<IExternalScriptsUI>()?.Show();
    }
}
