// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Globalization;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable session-specific state of the engine services and related data (aka saved game status).
    /// </summary>
    [Serializable]
    public class GameStateMap : StateMap
    {
        public DateTime SaveDateTime
        {
            get => string.IsNullOrEmpty(saveDateTime) ? DateTime.MinValue : DateTime.ParseExact(saveDateTime, dateTimeFormat, CultureInfo.InvariantCulture);
            set => saveDateTime = value.ToString(dateTimeFormat, CultureInfo.InvariantCulture);
        }
        public Texture2D Thumbnail
        {
            get => GetThumbnail();
            set => thumbnailBase64 = Convert.ToBase64String(value.EncodeToJPG());
        }

        private const string dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        [SerializeField] string saveDateTime;
        [SerializeField] string thumbnailBase64;

        private Texture2D GetThumbnail ()
        {
            if (string.IsNullOrEmpty(thumbnailBase64)) return Texture2D.whiteTexture;
            var tex = new Texture2D(2, 2);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.LoadImage(Convert.FromBase64String(thumbnailBase64));
            return tex;
        }
    } 
}
