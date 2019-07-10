// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Manages actors in the orthographic scene space.
    /// </summary>
    public abstract class OrthoActorManager<TActor, TState> : ActorManager<TActor, TState>
        where TActor : IActor
        where TState : ActorState<TActor>, new()
    {
        /// <summary>
        /// Scene origin point position in world space.
        /// </summary>
        public Vector2 GlobalSceneOrigin => SceneToWorldSpace(config.SceneOrigin);

        protected CameraManager OrthoCamera { get; private set; }

        private readonly OrthoActorManagerConfiguration config;

        public OrthoActorManager (OrthoActorManagerConfiguration config, CameraManager orthoCamera) 
            : base(config)
        {
            this.config = config;
            OrthoCamera = orthoCamera;
        }

        /// <summary>
        /// Converts ortho scene space position to world position.
        /// Scene space described as follows: x0y0 is at the bottom left and x1y1 is at the top right corner of the screen.
        /// </summary>
        public Vector2 SceneToWorldSpace (Vector2 scenePosition)
        {
            var originPosition = -OrthoCamera.ReferenceSize / 2f;
            return originPosition + Vector2.Scale(scenePosition, OrthoCamera.ReferenceSize);
        }

        /// <summary>
        /// Changes provided actor y position so that it's bottom edge is alligned with the bottom of the screen.
        /// </summary>
        public void MoveActorToBottom (TActor actor)
        {
            var metadata = GetActorMetadata<OrthoActorMetadata>(actor.Id);
            var bottomY = (metadata.Pivot.y * actor.Scale.y) / metadata.PixelsPerUnit - OrthoCamera.MaxOrthoSize;
            actor.ChangePositionY(bottomY);
        }
    }
}
