// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents data required to construct and initialize a <see cref="IActor"/> 
    /// managed in the orthographic scene space.
    /// </summary>
    [System.Serializable]
    public abstract class OrthoActorMetadata : ActorMetadata
    {
        [Tooltip("Pivot point of the actor.")]
        public Vector2 Pivot = Vector2.zero;
        [Tooltip("PPU value of the actor.")]
        public int PixelsPerUnit = 100;
    }
}
