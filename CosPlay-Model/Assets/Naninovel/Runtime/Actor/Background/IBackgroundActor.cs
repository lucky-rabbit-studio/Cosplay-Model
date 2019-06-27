// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a background actor on scene.
    /// </summary>
    public interface IBackgroundActor : IActor
    {
        /// <summary>
        /// Changes background appearance over specified time with specified transition effect.
        /// </summary>
        Task TransitionAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null);
    } 
}
