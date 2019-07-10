// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Stores engine version and build number.
    /// </summary>
    public class EngineVersion : ScriptableObject
    {
        public string Version => engineVersion;
        public string Build => buildDate;

        protected static string GitHubProjectPath => PlayerPrefs.GetString(nameof(GitHubProjectPath), string.Empty);

        private const string resourcesPath = "Naninovel/" + nameof(EngineVersion);

        [SerializeField, ReadOnly] private string engineVersion = string.Empty;
        [SerializeField, ReadOnly] private string buildDate = string.Empty;

        public static EngineVersion LoadFromResources ()
        {
            return Resources.Load<EngineVersion>(resourcesPath);
        }

        #if UNITY_EDITOR
        [ContextMenu("Update")]
        public void UpdateMenu () => Update();

        public static void Update ()
        {
            var asset = LoadFromResources();

            // Try resolve git version option #1.
            var gitPath = $"{GitHubProjectPath}/.git/refs/tags";
            if (Directory.Exists(gitPath)) asset.engineVersion = Directory.GetFiles(gitPath)?.Max()?.GetAfter(Path.DirectorySeparatorChar.ToString());

            // Try resolve git version option #2.
            if (string.IsNullOrWhiteSpace(asset.engineVersion))
            {
                gitPath = $"{GitHubProjectPath}/.git/packed-refs";
                if (File.Exists(gitPath)) asset.engineVersion = File.ReadAllText(gitPath).GetAfter("/").TrimFull();
            }

            asset.buildDate = $"{DateTime.Now:yyyy-MM-dd}";
            UnityEditor.EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public static DateTime ParseBuildDate (string buildDate)
        {
            var parsed = DateTime.TryParseExact(buildDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);
            return parsed ? result : DateTime.MinValue;
        }
        #endif
    }
}
