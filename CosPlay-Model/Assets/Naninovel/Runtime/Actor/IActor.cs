// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent an actor on scene.
    /// </summary>
    public interface IActor
    {
        /// <summary>
        /// Unique identifier of the actor. 
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Appearance of the actor. 
        /// </summary>
        string Appearance { get; set; }
        /// <summary>
        /// Whether the actor is currently visible on scene.
        /// </summary>
        bool IsVisible { get; set; }
        /// <summary>
        /// Position of the actor. 
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// Rotation of the actor. 
        /// </summary>
        Quaternion Rotation { get; set; }
        /// <summary>
        /// Scale of the actor. 
        /// </summary>
        Vector3 Scale { get; set; }
        /// <summary>
        /// Tint color of the actor. 
        /// </summary>
        Color TintColor { get; set; }

        /// <summary>
        /// Allows to perform an async initialization routine.
        /// Invoked once by <see cref="IActorManager"/> after actor is constructed.
        /// </summary>
        Task InitializeAsync ();

        /// <summary>
        /// Changes <see cref="Appearance"/> over specified time.
        /// </summary>
        Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default);
        /// <summary>
        /// Changes <see cref="IsVisible"/> over specified time.
        /// </summary>
        Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default);
        /// <summary>
        /// Changes <see cref="Position"/> over specified time.
        /// </summary>
        Task ChangePositionAsync (Vector3 position, float duration, EasingType easingType = default);
        /// <summary>
        /// Changes <see cref="Rotation"/> over specified time.
        /// </summary>
        Task ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default);
        /// <summary>
        /// Changes <see cref="Scale"/> factor over specified time.
        /// </summary>
        Task ChangeScaleAsync (Vector3 scale, float duration, EasingType easingType = default);
        /// <summary>
        /// Changes <see cref="TintColor"/> over specified time.
        /// </summary>
        Task ChangeTintColorAsync (Color tintColor, float duration, EasingType easingType = default);

        /// <summary>
        /// Loads (if not loaded) and invokes <see cref="Resource.Hold(object)"/> upon all resources required for the specified actor's appearance.
        /// </summary>
        Task HoldResourcesAsync (object holder, string appearance);
        /// <summary>
        /// Invokes <see cref="Resource.Release(object)"/> upon all resources required for the specified actor's appearance.
        /// </summary>
        void ReleaseResources (object holder, string appearance);
    }
}
