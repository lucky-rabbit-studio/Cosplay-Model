// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;
using UnityEngine;

namespace Naninovel.UI
{
    public class DebugInfoGUI : MonoBehaviour
    {
        public static KeyCode PreviousKey { get; set; } = KeyCode.LeftArrow;
        public static KeyCode NextKey { get; set; } = KeyCode.RightArrow;
        public static KeyCode PlayKey { get; set; } = KeyCode.UpArrow;
        public static KeyCode StopKey { get; set; } = KeyCode.DownArrow;

        private static bool initialized, show;
        private static Rect windowRect = new Rect(20, 20, 250, 125);
        private static EngineVersion version;
        private static NovelScriptPlayer player;
        private static AudioManager audioManager;
        private static string lastActionInfo, lastAutoVoiceName;

        public static void Toggle ()
        {
            if (!initialized)
            {
                var hostObject = new GameObject("NaninovelDebugInfoGUI");
                hostObject.hideFlags = HideFlags.HideAndDontSave;
                DontDestroyOnLoad(hostObject);
                hostObject.AddComponent<DebugInfoGUI>();
                initialized = true;
            }
            show = !show;
        }

        private void Awake ()
        {
            version = EngineVersion.LoadFromResources();
            player = Engine.GetService<NovelScriptPlayer>();
            audioManager = Engine.GetService<AudioManager>();
        }

        private void OnEnable ()
        {
            player.OnActionExecutionStart += HandleActionExecuted;
        }

        private void OnDisable ()
        {
            player.OnActionExecutionStart -= HandleActionExecuted;
        }

        private void Update ()
        {
            if (Input.GetKeyDown(PreviousKey)) player.SelectPrevious().WrapAsync();
            if (Input.GetKeyDown(NextKey)) player.SelectNext().WrapAsync();
            if (Input.GetKeyDown(PlayKey)) player.Play();
            if (Input.GetKeyDown(StopKey)) player.Stop();
        }

        private void OnGUI ()
        {
            if (!show) return;

            windowRect = GUI.Window(0, windowRect, DrawWindow, 
                string.IsNullOrEmpty(lastActionInfo) ? $"Naninovel ver. {version.Version}" : lastActionInfo);
        }

        private void DrawWindow (int windowID)
        {
            if (player.PlayedAction != null)
            {
                if (!string.IsNullOrEmpty(lastAutoVoiceName))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Auto Voice: ");
                    GUILayout.TextField(lastAutoVoiceName);
                    GUILayout.EndHorizontal();
                }

                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<< previous")) player.SelectPrevious().WrapAsync();
                if (GUILayout.Button("next >>")) player.SelectNext().WrapAsync();
                GUILayout.EndHorizontal();
                if (!player.IsPlaying && GUILayout.Button("Play")) player.Play();
                if (player.IsPlaying && GUILayout.Button("Stop")) player.Stop();
                if (GUILayout.Button("Close window")) show = false;
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void HandleActionExecuted (Actions.NovelAction action)
        {
            if (action is null) return;

            lastActionInfo = $"{player.PlayedScript.Name} #{player.PlayedAction.LineIndex}.{player.PlayedAction.InlineIndex}";

            if (audioManager != null && audioManager.AutoVoicingEnabled && action is Actions.PrintText printAction)
                lastAutoVoiceName = $"{player.PlayedScript.Name}/{printAction.LineNumber}.{printAction.InlineIndex}";
        }
    }
}
