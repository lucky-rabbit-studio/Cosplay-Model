// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    [System.Serializable]
    public abstract class OrthoActorManagerConfiguration : NovelActorManagerConfiguration
    {
        [Tooltip("Origin point used for reference when positioning actors on scene.")]
        public Vector2 SceneOrigin = new Vector2(.5f, 0f);
    }
}
