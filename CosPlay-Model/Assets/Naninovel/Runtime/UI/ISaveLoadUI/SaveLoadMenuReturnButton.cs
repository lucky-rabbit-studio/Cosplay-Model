// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel.UI
{
    public class SaveLoadMenuReturnButton : ScriptableButton
    {
        private SaveLoadMenu saveLoadMenu;

        protected override void Awake ()
        {
            base.Awake();

            saveLoadMenu = GetComponentInParent<SaveLoadMenu>();
        }

        protected override void OnButtonClick () => saveLoadMenu.Hide();
    }
}
