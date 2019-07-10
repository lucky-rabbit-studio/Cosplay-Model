// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityCommon;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class VoiceoverWindow : EditorWindow
    {
        protected string OutputPath { get => PlayerPrefs.GetString(outputPathKey); set => PlayerPrefs.SetString(outputPathKey, value); }
        protected bool UseMarkdownFormat { get => PlayerPrefs.GetInt(useMarkdownFormatKey) == 1; set => PlayerPrefs.SetInt(useMarkdownFormatKey, value ? 1 : 0); }

        private static readonly GUIContent localeLabel = new GUIContent("Locale");
        private static readonly GUIContent useMdLabel = new GUIContent("Use Markdown Format", "Whether to produce markdown (.md) instead of plain text (.txt) files with some formatting for better readability.");

        private const string outputPathKey = "Naninovel." + nameof(VoiceoverWindow) + "." + nameof(OutputPath);
        private const string useMarkdownFormatKey = "Naninovel." + nameof(VoiceoverWindow) + "." + nameof(UseMarkdownFormat);

        private bool isWorking = false;
        private ScriptManager scriptsManager;
        private LocalizationManager localizationManager;
        private string locale = null;

        [MenuItem("Naninovel/Tools/Voiceover Documents")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 150);
            GetWindowWithRect<VoiceoverWindow>(position, true, "Voiceover Documents", true);
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

            scriptsManager = Engine.GetService<ScriptManager>();
            localizationManager = Engine.GetService<LocalizationManager>();
            locale = localizationManager.DefaultLocale;
            isWorking = false;
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Voiceover Documents", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate voiceover documents.", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            if (isWorking)
            {
                EditorGUILayout.HelpBox("Working, please wait...", MessageType.Info);
                return;
            }

            locale = LocalesPopupDrawer.Draw(locale, localeLabel);
            UseMarkdownFormat = EditorGUILayout.Toggle(useMdLabel, UseMarkdownFormat);
            using (new EditorGUILayout.HorizontalScope())
            {
                OutputPath = EditorGUILayout.TextField("Output Path", OutputPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    OutputPath = EditorUtility.OpenFolderPanel("Output Path", "", "");
            }
            EditorGUILayout.Space();

            if (!localizationManager.IsLocaleAvailable(locale))
            {
                EditorGUILayout.HelpBox($"Selected locale is not available. Make sure a `{locale}` directory exists in the localization resources.", MessageType.Warning, true);
                return;
            }

            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(OutputPath));
            if (GUILayout.Button("Generate Voiceover Documents", GUIStyles.NavigationButton))
                GenerateVoiceoverDocumentsAsync();
            EditorGUI.EndDisabledGroup();
        }

        private async void GenerateVoiceoverDocumentsAsync ()
        {
            isWorking = true;

            await localizationManager.SelectLocaleAsync(locale);

            var scripts = await scriptsManager.LoadAllScriptsAsync();
            WriteVoiceoverDocuments(scripts);

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            isWorking = false;
            Repaint();
        }

        private void WriteVoiceoverDocuments (IEnumerable<Script> scripts)
        {
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);

            new DirectoryInfo(OutputPath).GetFiles().ToList().ForEach(f => f.Delete());

            foreach (var script in scripts)
            {
                var scriptText = $"# Voiceover document for script '{script.Name}' ({locale ?? "default"} locale)\n\n";
                var commands = script.CollectAllCommandLines()
                    .Select(l => Command.FromScriptLine(l, true))
                    .Where(cmd => cmd != null);
                foreach (var cmd in commands)
                {
                    if (!(cmd is PrintText)) continue;
                    var printCmd = cmd as PrintText;

                    scriptText += UseMarkdownFormat ? $"## {printCmd.AutoVoiceClipName}\n" : $"{printCmd.AutoVoiceClipName}\n";
                    if (!string.IsNullOrEmpty(printCmd.ActorId))
                        scriptText += $"{printCmd.ActorId}: ";
                    scriptText += UseMarkdownFormat ? $"`{printCmd.Text}`\n\n" : $"{printCmd.Text}\n\n";
                }

                var fileExtension = UseMarkdownFormat ? "md" : "txt";
                File.WriteAllText($"{OutputPath}/{script.Name}.{fileExtension}", scriptText, Encoding.UTF8);
            }
        }
    }
}
