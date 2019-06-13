// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent an actor on scene.
    /// </summary>
    public interface INovelActor
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
        /// Allows to execute any async initialization logic.
        /// Invoked once by <see cref="NovelActorManager"/> after actor is constructed.
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
        /// Loads resources required for specified actor's appearance.
        /// Will load the entire actor resources when appearance is not specified.
        /// </summary>
        Task PreloadResourcesAsync (string appearance = null);
        /// <summary>
        /// Unloads resources required for specified actor's appearance.
        /// Will unload the entire actor resources when appearance is not specified.
        /// </summary>
        Task UnloadResourcesAsync (string appearance = null);
    }
}
