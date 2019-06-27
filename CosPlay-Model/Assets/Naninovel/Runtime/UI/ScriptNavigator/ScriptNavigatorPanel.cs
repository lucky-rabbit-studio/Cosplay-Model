// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class ScriptNavigatorPanel : ScriptableUIBehaviour
    {
        [SerializeField] private Transform buttonsContainer = null;
        [SerializeField] private GameObject playButtonPrototype = null;

        protected ScriptPlayer Player { get; private set; }
        protected ScriptManager ScriptManager { get; private set; }
        protected bool LoadedScriptsOnce { get; private set; }

        protected override void Awake ()
        {
            base.Awake();
            this.AssertRequiredObjects(buttonsContainer, playButtonPrototype);
            Player = Engine.GetService<ScriptPlayer>();
            ScriptManager = Engine.GetService<ScriptManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            Player.OnPlay += Hide;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            Player.OnPlay -= Hide;
        }

        public override async Task SetIsVisibleAsync (bool isVisible, float? fadeTime = null)
        {
            await base.SetIsVisibleAsync(isVisible, fadeTime);

            if (isVisible && !LoadedScriptsOnce)
            {
                LoadedScriptsOnce = true;
                await LoadScriptsAsync();
            }
        }

        public void GenerateScriptButtons (IEnumerable<Script> scripts)
        {
            DestroyScriptButtons();

            foreach (var script in scripts)
            {
                var scriptButton = Instantiate(playButtonPrototype);
                scriptButton.transform.SetParent(buttonsContainer, false);
                scriptButton.GetComponent<NavigatorPlaytButton>().Initialize(this, script, Player);
            }
        }

        public void DestroyScriptButtons () => ObjectUtils.DestroyAllChilds(buttonsContainer);

        protected virtual async Task LoadScriptsAsync () => await ScriptManager.LoadAllScriptsAsync();
    } 
}
