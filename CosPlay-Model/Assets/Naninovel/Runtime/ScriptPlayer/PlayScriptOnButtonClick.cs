// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    /// <summary>
    /// Allows to play specified <see cref="Script"/> when <see cref="Button"/> is clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PlayScriptOnButtonClick : MonoBehaviour
    {
        [Tooltip("The script to play when the button is clicked.")]
        [ResourcesPopup(ScriptsConfiguration.DefaultScriptsPathPrefix, ScriptsConfiguration.DefaultScriptsPathPrefix, "None (disabled)")]
        [SerializeField] private string scriptName = default;
        [TextArea, Tooltip("The naninovel script text to execute when the button is clicked; has no effect when `Script Name` is specified.")]
        [SerializeField] private string scriptText = default;

        private Button button;
        private ScriptPlayer scriptPlayer;
        private ScriptManager scriptManager;

        private void Awake ()
        {
            button = GetComponent<Button>();
            scriptPlayer = Engine.GetService<ScriptPlayer>();
            scriptManager = Engine.GetService<ScriptManager>();
        }

        private void OnEnable ()
        {
            button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable ()
        {
            button.onClick.RemoveListener(HandleButtonClicked);
        }

        private async void HandleButtonClicked ()
        {
            button.interactable = false;

            if (!string.IsNullOrEmpty(scriptName))
            {
                var script = await scriptManager.LoadScriptAsync(scriptName);
                if (script is null)
                {
                    Debug.LogError($"Failed to play `{scriptName}` for the on-click script of button `{name}`. Make sure the specified script exists.");
                    button.interactable = true;
                    return;
                }
                await scriptPlayer.PreloadAndPlayAsync(script);
            }
            else if (!string.IsNullOrWhiteSpace(scriptText))
            {
                var script = new Script(name, scriptText);
                var playlist = new ScriptPlaylist(script);
                foreach (var command in playlist)
                    await command.ExecuteAsync();
            }

            button.interactable = true;
        }
    }
}
