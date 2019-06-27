// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.UI
{
    public class ExternalScriptsBrowserPanel : ScriptNavigatorPanel, IExternalScriptsUI
    {
        public Task InitializeAsync () => Task.CompletedTask;

        protected override async Task LoadScriptsAsync ()
        {
            var scripts = await ScriptManager.LoadExternalScriptsAsync();
            GenerateScriptButtons(scripts);
        }
    } 
}
