// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    /// <summary>
    /// Allows to play specified <see cref="NovelScript"/> when <see cref="Button"/> is clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PlayScriptOnButtonClick : MonoBehaviour
    {
        [Tooltip("The script to play when the button is clicked.")]
        [ResourcesPopup(ScriptsConfiguration.DefaultScriptsPathPrefix, ScriptsConfiguration.DefaultScriptsPathPrefix, "None (disabled)")]
        [SerializeField] private string scriptName = default;
        [TextArea, Tooltip("The novel script text to execute when the button is clicked; has no effect when `Script Name` is specified.")]
        [SerializeField] private string scriptText = default;

        private Button button;
        private NovelScriptPlayer scriptPlayer;
        private NovelScriptManager scriptManager;

        private void Awake ()
        {
            button = GetComponent<Button>();
            scriptPlayer = Engine.GetService<NovelScriptPlayer>();
            scriptManager = Engine.GetService<NovelScriptManager>();
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
                var novelScript = await scriptManager.LoadScriptAsync(scriptName);
                if (novelScript is null)
                {
                    Debug.LogError($"Failed to play `{scriptName}` for the on-click script of button `{name}`. Make sure the specified script exists.");
                    button.interactable = true;
                    return;
                }
                await scriptPlayer.PreloadAndPlayAsync(novelScript);
            }
            else if (!string.IsNullOrWhiteSpace(scriptText))
            {
                var novelScript = new NovelScript(name, scriptText);
                var playlist = new NovelScriptPlaylist(novelScript);
                foreach (var action in playlist)
                    await action.ExecuteAsync();
            }

            button.interactable = true;
        }
    }
}
