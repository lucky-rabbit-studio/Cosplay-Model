// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a naninovel script imported as Unity asset.
    /// </summary>
    public class ScriptAsset : ScriptableObject
    {
        /// <summary>
        /// The contents of the imported naninovel script.
        /// </summary>
        public string ScriptText => scriptText;

        [SerializeField] private string scriptText = default;

        public static ScriptAsset FromScriptText (string scriptText)
        {
            var asset = CreateInstance<ScriptAsset>();
            asset.scriptText = scriptText;
            return asset;
        }
    }
}
