// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using UnityCommon;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// On build pre-process: iterate all the records from <see cref="EditorResources"/> asset, copying the referenced assets 
    /// to a temp `Resources` folder; assets already stored by the same path in other `Resources` folders will be ignored.
    /// TODO: In case another asset is stored by the path -- it should be temporarily moved out of the `Resources` folder.
    /// On build post-process or build fail: restore any affected assets and delete the created temporary `Resources` folder.
    /// </summary>
    public class ResourcesBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 10;

        private const string tempResourcesPath = "Assets/TEMP_NANINOVEL/Resources";
        private const string tempStreamingPath = "Assets/StreamingAssets";

        public void OnPreprocessBuild (BuildReport report)
        {
            EditorUtils.CreateFolderAsset(tempResourcesPath);

            var records = EditorResources.LoadOrDefault().GetAllRecords();
            var projectResources = ProjectResources.Get();
            var progress = 0;
            foreach (var record in records)
            {
                progress++;

                var resourcePath = record.Key;
                var resourceAssetPath = AssetDatabase.GUIDToAssetPath(record.Value);
                if (string.IsNullOrEmpty(resourceAssetPath))
                {
                    Debug.LogWarning($"Failed to resolve `{resourcePath}` asset path from GUID stored in `EditorResources` asset. The resource won't be included to the build.");
                    continue;
                }
                
                var resourceAsset = AssetDatabase.LoadAssetAtPath<Object>(resourceAssetPath);
                if (string.IsNullOrEmpty(resourceAssetPath))
                {
                    Debug.LogWarning($"Failed to load `{resourcePath}` asset. The resource won't be included to the build.");
                    continue;
                }

                if (EditorUtility.DisplayCancelableProgressBar("Processing Naninovel Resources", $"Processing '{resourceAssetPath}' asset...", progress / (float)records.Count))
                {
                    OnPostprocessBuild(null); // Remove temporary assets.
                    throw new System.OperationCanceledException("Build was cancelled by the user.");
                }

                if (resourceAsset is SceneAsset)
                    ProcessSceneResource(resourcePath, resourceAsset as SceneAsset);
                if (resourceAsset is UnityEngine.Video.VideoClip && report.summary.platform == BuildTarget.WebGL)
                    ProcessVideoResourceForWebGL(resourcePath, resourceAsset);
                else ProcessResourceAsset(resourcePath, resourceAsset, projectResources);
            }

            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }

        public void OnPostprocessBuild (BuildReport report)
        {
            AssetDatabase.DeleteAsset(tempResourcesPath.GetBeforeLast("/"));
            AssetDatabase.SaveAssets();
        }

        private void ProcessResourceAsset (string path, Object asset, ProjectResources projectResources)
        {
            if (projectResources.ResourcePaths.Contains(path)) return; // Skip assets stored in `Resources`.

            var objPath = AssetDatabase.GetAssetPath(asset);
            var resourcePath = PathUtils.Combine(tempResourcesPath, path);
            if (objPath.Contains(".")) resourcePath += $".{objPath.GetAfter(".")}";

            EditorUtils.CreateFolderAsset(resourcePath.GetBeforeLast("/"));
            AssetDatabase.CopyAsset(objPath, resourcePath);
        }

        /// <summary>
        /// Make sure the scene is included to the build settings.
        /// </summary>
        private void ProcessSceneResource (string path, SceneAsset sceneAsset)
        {
            var currentScenes = EditorBuildSettings.scenes.ToList();
            var scenePath = AssetDatabase.GetAssetPath(sceneAsset);
            if (string.IsNullOrEmpty(scenePath) || currentScenes.Exists(s => s.path == scenePath)) return;

            currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = currentScenes.ToArray();
        }

        /// <summary>
        /// Copy video to `StreamingAssets` folder for streaming on WebGL (built-in videos are not supported on the platform).
        /// </summary>
        private void ProcessVideoResourceForWebGL (string path, Object asset)
        {
            var objPath = AssetDatabase.GetAssetPath(asset);
            var streamingPath = PathUtils.Combine(tempStreamingPath, path);
            if (objPath.Contains(".")) streamingPath += $".{objPath.GetAfter(".")}";
            if (objPath.EndsWithFast(streamingPath))
                return; // The file is already in a streaming assets folder.

            EditorUtils.CreateFolderAsset(streamingPath.GetBeforeLast("/"));
            AssetDatabase.CopyAsset(objPath, streamingPath);
        }
    }
}
