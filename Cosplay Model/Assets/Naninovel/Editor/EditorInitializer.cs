// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Naninovel
{
    public static class EditorInitializer
    {
        public static async Task InitializeAsync ()
        {
            if (Engine.IsInitialized) return;

            var engineConfig = Configuration.LoadOrDefault<EngineConfiguration>();
            var behaviour = new EditorBehaviour();
            var services = new List<IEngineService>();

            var providersManager = new ResourceProviderManager(Configuration.LoadOrDefault<ResourceProviderConfiguration>());
            services.Add(providersManager);

            var localizationManager = new LocalizationManager(Configuration.LoadOrDefault<LocalizationConfiguration>(), providersManager);
            services.Add(localizationManager);

            var scriptsManager = new NovelScriptManager(Configuration.LoadOrDefault<ScriptsConfiguration>(), providersManager, localizationManager);
            services.Add(scriptsManager);

            var varsManager = new CustomVariableManager();
            services.Add(varsManager);

            await Engine.InitializeAsync(engineConfig, behaviour, services);
        }
    }
}
