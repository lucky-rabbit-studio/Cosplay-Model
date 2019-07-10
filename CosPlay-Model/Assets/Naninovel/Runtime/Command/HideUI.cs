// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Makes a [managed UI](/guide/ui-customization.md) with the provided prefab name invisible.
    /// </summary>
    /// <example>
    /// ; Given you've added a custom managed UI with prefab name `Calendar`,
    /// ; the following will make it invisible on the scene.
    /// @hideUI Calendar
    /// </example>
    public class HideUI : Command
    {
        private struct UndoData { public bool Executed; public string UIPrefabName; }

        /// <summary>
        /// Name of the managed UI prefab to hide.
        /// </summary>
        [CommandParameter(NamelessParameterAlias)]
        public string UIPrefabName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override Task ExecuteAsync ()
        {
            var uiManager = Engine.GetService<UIManager>();
            var ui = uiManager.GetUI(UIPrefabName);

            if (ui is null)
            {
                Debug.LogWarning($"Failed to execute {nameof(HideUI)} script command: managed UI with prefab name `{UIPrefabName}` not found.");
                return Task.CompletedTask;
            }

            undoData.Executed = true;
            undoData.UIPrefabName = UIPrefabName;

            ui.Hide();

            return Task.CompletedTask;
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var uiManager = Engine.GetService<UIManager>();
            var ui = uiManager.GetUI(undoData.UIPrefabName);

            if (ui is null)
                Debug.LogWarning($"Failed to undo {nameof(HideUI)} script command: managed UI with prefab name `{undoData.UIPrefabName}` not found.");
            else ui.IsVisible = true;

            undoData = default;
            return Task.CompletedTask;
        }
    }
}
