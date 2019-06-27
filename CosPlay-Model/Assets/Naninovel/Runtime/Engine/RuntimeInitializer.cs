// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public class RuntimeInitializer : MonoBehaviour
    {
        [SerializeField] private bool initializeOnAwake = true;

        private const string defaultInitUIResourcesPath = "Naninovel/EngineInitializationUI";
        private static EngineConfiguration engineConfig;

        private void Awake ()
        {
            if (initializeOnAwake) InitializeAsync().WrapAsync();
        }

        public static async Task InitializeAsync ()
        {
            if (Engine.IsInitialized) return;

            var initializationUI = default(ScriptableUIBehaviour);
            if (engineConfig.ShowInitializationUI)
            {
                var initUIPrefab = ObjectUtils.IsValid(engineConfig.CustomInitializationUI) ? engineConfig.CustomInitializationUI : Resources.Load<ScriptableUIBehaviour>(defaultInitUIResourcesPath);
                initializationUI = Instantiate(initUIPrefab);
                initializationUI.Show();
            }

            var behaviour = RuntimeBehaviour.Create();
            var services = new List<IEngineService>();

            bool isService (Type t) => typeof(IEngineService).IsAssignableFrom(t);
            bool isBehaviour (Type t) => typeof(IEngineBehaviour).IsAssignableFrom(t);
            bool isConfig (Type t) => typeof(Configuration).IsAssignableFrom(t);
            var attrType = typeof(InitializeAtRuntimeAttribute);
            var serviceTypes = ReflectionUtils.ExportedDomainTypes.Where(type => type.IsDefined(attrType, false));
            serviceTypes = serviceTypes.OrderBy(t => (t.GetCustomAttributes(attrType, false).First() as InitializeAtRuntimeAttribute).InitializationPriority);
            serviceTypes = serviceTypes.TopologicalOrder(t => t.GetConstructors().First().GetParameters().Where(p => isService(p.ParameterType)).Select(p => p.ParameterType));
            var ctorParams = new List<object>();
            foreach (var serviceType in serviceTypes)
            {
                var paramTypes = serviceType.GetConstructors().First().GetParameters().Select(p => p.ParameterType);
                foreach (var paramType in paramTypes)
                    if (isService(paramType) && serviceTypes.Contains(paramType)) ctorParams.Add(services.First(s => s.GetType() == paramType));
                    else if (isBehaviour(paramType)) ctorParams.Add(behaviour);
                    else if (isConfig(paramType)) ctorParams.Add(Configuration.LoadOrDefault(paramType));
                    else Debug.LogError($"Only `{nameof(Configuration)}`, `{nameof(IEngineBehaviour)}` and `{nameof(IEngineService)}` with an `{nameof(InitializeAtRuntimeAttribute)}` can be requested in an engine service constructor.");
                var service = Activator.CreateInstance(serviceType, ctorParams.ToArray()) as IEngineService;
                services.Add(service);
                ctorParams.Clear();
            }

            await Engine.InitializeAsync(engineConfig, behaviour, services);

            if (initializationUI)
            {
                await initializationUI.SetIsVisibleAsync(false);
                Destroy(initializationUI.gameObject);
            }

            var moviePlayer = Engine.GetService<MoviePlayer>();
            if (moviePlayer.PlayIntroMovie)
                await moviePlayer.PlayAsync(moviePlayer.IntroMovieName);

            var scriptsConfig = Configuration.LoadOrDefault<ScriptsConfiguration>();
            if (!string.IsNullOrEmpty(scriptsConfig.InitializationScript))
                await Engine.GetService<ScriptPlayer>()?.PreloadAndPlayAsync(scriptsConfig.InitializationScript);

            if (engineConfig.ShowTitleUI)
                Engine.GetService<UIManager>()?.GetUI<UI.ITitleUI>()?.Show();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationLoaded ()
        {
            engineConfig = Configuration.LoadOrDefault<EngineConfiguration>();
            if (engineConfig.InitializeOnApplicationLoad)
                InitializeAsync().WrapAsync();
        }
    }
}
