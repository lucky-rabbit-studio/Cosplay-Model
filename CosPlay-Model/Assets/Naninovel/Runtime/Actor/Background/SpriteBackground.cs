// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="SpriteActor"/> to represent an actor.
    /// </summary>
    public class SpriteBackground : SpriteActor, IBackgroundActor
    {
        public SpriteBackground (string id, BackgroundMetadata metadata) 
            : base(id, metadata) { }

        public async Task TransitionAppearanceAsync (string appearance, float duration, EasingType easingType = default, 
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null)
        {
            if (transitionType.HasValue) SpriteRenderer.TransitionType = transitionType.Value;
            if (transitionParams.HasValue) SpriteRenderer.TransitionParams = transitionParams.Value;
            if (ObjectUtils.IsValid(customDissolveTexture)) SpriteRenderer.CustomDissolveTexture = customDissolveTexture;

            await ChangeAppearanceAsync(appearance, duration, easingType);
        }
    } 
}
