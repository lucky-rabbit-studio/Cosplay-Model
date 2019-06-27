// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class TitleExternalScriptsButton : ScriptableButton
    {
        private UIManager uiManager;
        private ScriptManager scriptManager;

        protected override void Awake ()
        {
            base.Awake();

            scriptManager = Engine.GetService<ScriptManager>();
            uiManager = Engine.GetService<UIManager>();
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
