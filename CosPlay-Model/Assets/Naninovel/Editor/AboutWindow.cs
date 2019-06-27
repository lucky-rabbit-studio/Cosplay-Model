// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.IO;
using UnityCommon;
using UnityEditor;
using UnityEngine;

namespace Naninovel
{
    public class AboutWindow : EditorWindow
    {
        public static string InstalledVersion { get => PlayerPrefs.GetString(installedVersionKey); set => PlayerPrefs.SetString(installedVersionKey, value); }

        private const string installedVersionKey = "Naninovel." + nameof(AboutWindow) + "." + nameof(InstalledVersion);
        private const string guideUri = "https://naninovel.com/guide/";
        private const string apiReferenceUri = "https://naninovel.com/api/";
        private const string issueTrackerUri = "https://github.com/Elringus/NaninovelWeb/issues?q=is%3Aissue+label%3Abug";
        private const string discordUri = "https://discord.gg/avhRzP3";
        private const string supportUri = "https://naninovel.com/support/";
        private const string reviewUri = "https://u3d.as/1pg9";

        private EngineVersion engineVersion;
        private GUIContent logoContent;

        private void OnEnable ()
        {
            engineVersion = EngineVersion.LoadFromResources();
            InstalledVersion = engineVersion.Version;
            var logoPath = PathUtils.AbsoluteToAssetPath(Path.Combine(PackagePath.EditorResourcesPath, "NaninovelLogo.png"));
            logoContent = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture2D>(logoPath));
        }

        public void OnGUI ()
        {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(80);
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            var currentColor = GUI.contentColor;
            GUI.contentColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            GUILayout.Label(logoContent, GUIStyle.none);
            GUI.contentColor = currentColor;

            GUILayout.Space(5);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            EditorGUILayout.SelectableLabel($"{engineVersion.Version} build {engineVersion.Build}");
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Online Resources", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Check our online documentation for the quick start guides and tutorials. API reference will help you navigate through available script commands and the ways to use them. Bug reports, questions and suggestions are always welcome at the issue tracker and discord server.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Guide")) Application.OpenURL(guideUri);
            if (GUILayout.Button("API Reference")) Application.OpenURL(apiReferenceUri);
            if (GUILayout.Button("Issue Tracker")) Application.OpenURL(issueTrackerUri);
            if (GUILayout.Button("Discord")) Application.OpenURL(discordUri);
            if (GUILayout.Button("Support")) Application.OpenURL(supportUri);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rate Naninovel", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("We really hope you like Naninovel! If you feel like it, please leave a review on the Asset Store, that helps us out a lot.", EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Review On Asset Store")) Application.OpenURL(reviewUri);

            GUILayout.Space(5);
        }

        [InitializeOnLoadMethod]
        private static void FirstTimeSetup ()
        {
            EditorApplication.delayCall += ExecuteFirstTimeSetup;
        }

        private static void ExecuteFirstTimeSetup ()
        {
            EditorApplication.delayCall -= ExecuteFirstTimeSetup;

            // First time ever launch.
            if (string.IsNullOrWhiteSpace(InstalledVersion))
            {
                OpenWindow();
                return;
            }

            // First time after update launch.
            var engineVersion = EngineVersion.LoadFromResources();
            if (engineVersion && engineVersion.Version != InstalledVersion)
                OpenWindow();
        }

        [MenuItem("Naninovel/About", priority = 1)]
        private static void OpenWindow ()
        {
            var position = new Rect(100, 100, 375, 395);
            GetWindowWithRect<AboutWindow>(position, true, "About Naninovel", true);
        }
    }
}
