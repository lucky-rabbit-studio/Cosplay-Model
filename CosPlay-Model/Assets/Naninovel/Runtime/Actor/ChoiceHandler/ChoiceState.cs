// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a choice in <see cref="ChoiceHandlerState"/>.
    /// </summary>
    [Serializable]
    public class ChoiceState
    {
        /// <summary>
        /// GUID of the state instance.
        /// </summary>
        public string Id => id;
        /// <summary>
        /// Text describing consequences of this choice.
        /// </summary>
        public string Summary = null;
        /// <summary>
        /// Path (relative to a `Resources` folder) to a button prefab representing the choice.
        /// </summary>
        public string ButtonPath = null;
        /// <summary>
        /// Local position of the choice button inside the choice handler.
        /// </summary>
        public Vector2 ButtonPosition = default;
        /// <summary>
        /// Whether to apply <see cref="ButtonPosition"/> (whether user provided a custom position in the script command).
        /// </summary>
        public bool OverwriteButtonPosition = false;
        /// <summary>
        /// The choice will load script with the specified name.
        /// </summary>
        public string GotoScript = null;
        /// <summary>
        /// The choice will lead to label with the specified name.
        /// </summary>
        public string GotoLabel = null;
        /// <summary>
        /// The choice will lead to label with the specified name.
        /// </summary>
        public string SetExpression = null;
        /// <summary>
        /// Undo data of the master <see cref="Commands.AddChoice"/> command.
        /// </summary>
        public readonly Commands.AddChoice.UndoData UndoData = default;

        [SerializeField] string id;

        public ChoiceState (string summary = null, string buttonPath = null, Commands.AddChoice.UndoData undoDdata = null, Vector2? buttonPosition = null,
            string gotoScript = null, string gotoLabel = null, string setExpression = null)
        {
            id = Guid.NewGuid().ToString();
            Summary = summary;
            UndoData = undoDdata;
            ButtonPath = buttonPath;
            ButtonPosition = buttonPosition ?? default;
            OverwriteButtonPosition = buttonPosition.HasValue;
            GotoScript = gotoScript;
            GotoLabel = gotoLabel;
            SetExpression = setExpression;
        }
    }
}
