// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using UnityCommon;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Naninovel.Actions;

namespace Naninovel
{
    public class LocalizationWindow : EditorWindow
    {
        protected string LocaleFolderPath { get => PlayerPrefs.GetString(localeFolderPathKey); set => PlayerPrefs.SetString(localeFolderPathKey, value); }

        private const string localeFolderPathKey = "Naninovel." + nameof(LocalizationWindow) + "." + nameof(LocaleFolderPath);
        private bool isWorking = false;
        private bool tryUpdate = true;
        private bool localizeText = true;
        private ResourceProviderManager providersManager;
        private ResourceProviderType resourceProvider = ResourceProviderType.Project;
        private string scriptsPathPrefix = ScriptsConfiguration.DefaultScriptsPathPrefix;
        private string textPathPrefix = ManagedTextConfiguration.DefaultManagedTextPathPrefix;

        [MenuItem("Naninovel/Tools/Localization")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 200);
            GetWindowWithRect<LocalizationWindow>(position, true, "Localization", true);
        }

        private void OnEnable ()
        {
            if (!Engine.IsInitialized)
            {
                isWorking = true;
                Engine.OnInitialized += InializeEditor;
                EditorInitializer.InitializeAsync().WrapAsync();
            }
            else InializeEditor();
        }

        private void OnDisable ()
        {
            Engine.Destroy();
        }

        private void InializeEditor ()
        {
            Engine.OnInitialized -= InializeEditor;

            providersManager = Engine.GetService<ResourceProviderManager>();
            isWorking = false;
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Localization", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate localization resources.", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (isWorking)
            {
                EditorGUILayout.HelpBox("Working, please wait...", MessageType.Info);
                return;
            }

            resourceProvider = (ResourceProviderType)EditorGUILayout.EnumPopup("Source Provider", resourceProvider);
            scriptsPathPrefix = EditorGUILayout.TextField("Scripts Path Prefix", scriptsPathPrefix);
            textPathPrefix = EditorGUILayout.TextField("Text Path Prefix", textPathPrefix);
            using (new EditorGUILayout.HorizontalScope())
            {
                LocaleFolderPath = EditorGUILayout.TextField("Locale Folder", LocaleFolderPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    LocaleFolderPath = EditorUtility.OpenFolderPanel("Locale Folder Path", "", "");
            }
            tryUpdate = EditorGUILayout.Toggle("Try Update", tryUpdate);
            localizeText = EditorGUILayout.Toggle("Localize Text", localizeText);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Localization Resources", GUIStyles.NavigationButton))
                GenerateLocalizatonScriptsAsync();
        }

        private async void GenerateLocalizatonScriptsAsync ()
        {
            isWorking = true;

            var sourceScripts = await LoadSourceScriptsAsync(resourceProvider, scriptsPathPrefix);
            WriteLocalizationScripts(sourceScripts, scriptsPathPrefix);

            if (localizeText)
            {
                sourceScripts = await LoadSourceScriptsAsync(resourceProvider, textPathPrefix);
                WriteLocalizationScripts(sourceScripts, textPathPrefix);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            isWorking = false;
            Repaint();
        }

        private async Task<List<NovelScript>> LoadSourceScriptsAsync (ResourceProviderType providerType, string pathPrefix)
        {
            Debug.Assert(providersManager != null);
            await providersManager.GetProviderList(providerType).UnloadResourcesAsync();
            var resources = await providersManager.GetProviderList(providerType).LoadResourcesAsync<TextAsset>(pathPrefix);
            return resources.Select(r => new NovelScript(r.Path.Contains("/") ? r.Path.GetAfter("/") : r.Path, r.Object.text)).ToList();
        }

        private void WriteLocalizationScripts (List<NovelScript> sourceScripts, string pathPrefix)
        {
            var outputPath = $"{LocaleFolderPath}/{pathPrefix}";
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            var existingLocScripts = Directory.EnumerateFiles(outputPath, "*.txt")
                    .Select(path => new NovelScript(Path.GetFileName(path).GetBeforeLast(".txt"), File.ReadAllText(path))).ToList();
            new DirectoryInfo(outputPath).GetFiles().ToList().ForEach(f => f.Delete());

            foreach (var sourceScript in sourceScripts)
            {
                var scriptText = $"; Localization resource for script '{sourceScript.Name}'\n\n";
                var existingLocScript = existingLocScripts.FirstOrDefault(s => s.Name == sourceScript.Name);
                var existingLocTerms = existingLocScript != null ? NovelScriptLocalization.GenerateLocalizationTerms(existingLocScript) : null;
                foreach (var line in sourceScript.Lines)
                {
                    if (!NovelAction.IsLineLocalizable(line)) continue;
                    if (tryUpdate && existingLocTerms != null && existingLocTerms.ContainsKey(line.ContentHash))
                        scriptText += $"{GenerateTerm(line, existingLocTerms[line.ContentHash])}\n";
                    else scriptText += $"{GenerateTerm(line)}\n";
                }
                File.WriteAllText($"{outputPath}/{sourceScript.Name}.txt", scriptText, Encoding.UTF8);
            }
        }

        private static string GenerateTerm (NovelScriptLine scriptLine, List<string> existingLocTerms = null)
        {
            var term = $"# {scriptLine.ContentHash}\n; {scriptLine.Text}\n";
            if (existingLocTerms != null)
            {
                foreach (var termText in existingLocTerms)
                    term += $"{termText}\n";
            }
            return term;
        }
    }
}
