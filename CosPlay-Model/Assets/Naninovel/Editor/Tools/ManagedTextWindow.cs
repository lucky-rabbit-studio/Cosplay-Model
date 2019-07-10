// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityCommon;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class ManagedTextWindow : EditorWindow
    {
        protected string OutputPath { get => PlayerPrefs.GetString(outputPathKey); set => PlayerPrefs.SetString(outputPathKey, value); }

        private const string outputPathKey = "Naninovel." + nameof(ManagedTextWindow) + "." + nameof(OutputPath);
        private bool isWorking = false;
        private bool deleteUnusedDocuments = true;

        [MenuItem("Naninovel/Tools/Managed Text")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 125);
            GetWindowWithRect<ManagedTextWindow>(position, true, "Managed Text", true);
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Naninovel Managed Text", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool to generate managed text documents.", EditorStyles.miniLabel);

            EditorGUILayout.Space();

            if (isWorking)
            {
                EditorGUILayout.HelpBox("Working, please wait...", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                OutputPath = EditorGUILayout.TextField("Output Path", OutputPath);
                if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                    OutputPath = EditorUtility.OpenFolderPanel("Output Path", "", "");
            }
            deleteUnusedDocuments = EditorGUILayout.Toggle("Delete Unused", deleteUnusedDocuments);

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Managed Text Documents", GUIStyles.NavigationButton))
                GenerateDocuments();
        }

        private void GenerateDocuments ()
        {
            isWorking = true;

            var managedTextSet = ManagedTextUtils.GetManagedTextFromAssembly();
            var categoryToTextMap = managedTextSet.GroupBy(t => t.Category).ToDictionary(t => t.Key, t => new HashSet<ManagedText>(t));

            foreach (var kv in categoryToTextMap)
                ProcessDocumentCategory(kv.Key, kv.Value);

            if (deleteUnusedDocuments)
                DeleteUnusedDocuments(categoryToTextMap.Keys.ToList());

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            isWorking = false;
            Repaint();
        }

        private void ProcessDocumentCategory (string category, HashSet<ManagedText> documents)
        {
            var fullPath = $"{OutputPath}/{category}.nani";

            // Try to update existing resource.
            if (File.Exists(fullPath))
            {
                var existingScript = new Script(category, File.ReadAllText(fullPath));
                var existingTextSet = ManagedTextUtils.GetManagedTextFromScript(existingScript);
                // Remove existing fields no longer associated with the category (possibly moved to another or deleted).
                existingTextSet.RemoveWhere(t => !documents.Contains(t));
                // Remove new fields that already exist in the updated document, to prevent overriding.
                documents.ExceptWith(existingTextSet);
                // Add existing fields to the new set.
                documents.UnionWith(existingTextSet);
                File.Delete(fullPath);
            }

            var resultString = string.Empty;
            foreach (var managedText in documents)
            {
                if (!string.IsNullOrEmpty(managedText.Comment))
                    resultString += $"; {managedText.Comment}{Environment.NewLine}";
                resultString += $"{managedText.FieldId}: {managedText.FieldValue}{Environment.NewLine}";
            }

            File.WriteAllText(fullPath, resultString);
        }

        private void DeleteUnusedDocuments (List<string> usedCategories)
        {
            // Prevent deleting tips.
            usedCategories.Add(UI.TipsPanel.DefaultManagedTextCategory);
            foreach (var filePath in Directory.EnumerateFiles(OutputPath, "*.nani"))
                if (!usedCategories.Contains(Path.GetFileName(filePath).GetBeforeLast(".nani")))
                    File.Delete(filePath);
        }
    }
}
