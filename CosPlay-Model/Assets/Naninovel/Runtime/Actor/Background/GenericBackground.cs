// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IBackgroundActor"/> implementation using <see cref="BackgroundActorBehaviour"/> to represent an actor.
    /// </summary>
    /// <remarks>
    /// Resource prefab should have a <see cref="BackgroundActorBehaviour"/> component attached to the root object.
    /// Apperance and other property changes changes are routed to the events of the <see cref="BackgroundActorBehaviour"/> component.
    /// </remarks>
    public class GenericBackground : GenericActor<BackgroundActorBehaviour>, IBackgroundActor
    {
        public GenericBackground (string id, BackgroundMetadata metadata)
            : base(id, metadata) { }

        public async Task TransitionAppearanceAsync (string appearance, float duration, EasingType easingType = default,
            TransitionType? transitionType = null, Vector4? transitionParams = null, Texture customDissolveTexture = null)
        {
            await ChangeAppearanceAsync(appearance, duration, easingType);
        }
    }
}
