// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Modifies a [background actor](/guide/backgrounds.md).
    /// </summary>
    /// <remarks>
    /// Backgrounds are handled a bit differently from characters. Most of the time we'll only have
    /// one background actor on scene, which will constantly transition to different appearances.
    /// To free the user from always repeating same actor ID in scripts, we allow to
    /// provide only the background appearance and transition type (optional) as a nameless parameter and assume that
    /// `MainBackground` actor should be affected. When this is not the case, ID of the background actor can be explicitly
    /// provided via the `id` parameter.
    /// </remarks>
    /// <example>
    /// ; Set `River` as the appearance of the main background
    /// @back River
    /// 
    /// ; Same as above, but also use a `RadialBlur` transition effect
    /// @back River.RadialBlur
    /// 
    /// ; Given an `ExplosionSound` SFX and an `ExplosionSprite` background, the following 
    /// ; script sequence will simulate two explosions appearing far and close to the camera.
    /// @sfx ExplosionSound volume:0.1
    /// @back id:ExplosionSprite scale:0.3 pos:55,60 time:0 isVisible:false
    /// @back id:ExplosionSprite
    /// @fx ShakeBackground params:,1
    /// @hide ExplosionSprite
    /// @sfx ExplosionSound volume:1.5
    /// @back id:ExplosionSprite pos:65 scale:1
    /// @fx ShakeBackground params:,3
    /// @hide ExplosionSprite
    /// </example>
    [ActionAlias("back")]
    public class ModifyBackground : ModifyOrthoActor<IBackgroundActor, BackgroundState, BackgroundManager>
    {
        /// <summary>
        /// Appearance to set for the modified background and name of the [transition effect](/guide/background-transition-effects.md) to use.
        /// When transition is not provided, a cross-fade effect will be used by default.
        /// </summary>
        [ActionParameter(NamelessParameterAlias, true)]
        public Named<string> AppearanceAndTransition { get => GetDynamicParameter<Named<string>>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Parameters of the transition effect.
        /// </summary>
        [ActionParameter("params", true)]
        public float?[] TransitionParams { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Path to the [custom dissolve](/guide/background-transition-effects.md#custom-transition-effects) texture (path should be relative to a `Resources` folder).
        /// Has effect only when the transition is set to `Custom` mode.
        /// </summary>
        [ActionParameter("dissolve", true)]
        public string CustomDissolveTexturePath { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        [ActionParameter(optional: true)] // Default to main background when no ID is specified.
        public override string Id { get => base.Id ?? BackgroundManager.MainActorId; set => base.Id = value; }
        [ActionParameter(optional: true)] // Allows specifying background appearance as nameless parameter.
        public override string Appearance { get => AppearanceAndTransition?.Item1 ?? base.Appearance; set => base.Appearance = value; }

        private Texture2D preloadedCustomDissolveTexture;

        public override async Task PreloadResourcesAsync ()
        {
            await base.PreloadResourcesAsync();

            if (!string.IsNullOrEmpty(CustomDissolveTexturePath))
            {
                var loader = Resources.LoadAsync<Texture2D>(CustomDissolveTexturePath);
                await loader;
                preloadedCustomDissolveTexture = loader.asset as Texture2D;
            }
        }

        public override async Task UnloadResourcesAsync ()
        {
            await base.UnloadResourcesAsync();
            preloadedCustomDissolveTexture = null;
        }

        protected override async Task ApplyAppearanceModificationAsync (IBackgroundActor actor, EasingType easingType)
        {
            if (string.IsNullOrWhiteSpace(Appearance)) return;

            var transitionType = TransitionType.Crossfade;
            if (!string.IsNullOrWhiteSpace(AppearanceAndTransition?.Item2))
                transitionType = TransitionUtils.TypeFromString(AppearanceAndTransition.Item2);
            var defaultParams = transitionType.GetDefaultParams();
            var transitionParams = new Vector4(
                    TransitionParams?.ElementAtOrDefault(0) ?? defaultParams.x,
                    TransitionParams?.ElementAtOrDefault(1) ?? defaultParams.y,
                    TransitionParams?.ElementAtOrDefault(2) ?? defaultParams.z,
                    TransitionParams?.ElementAtOrDefault(3) ?? defaultParams.w);

            if (!string.IsNullOrEmpty(CustomDissolveTexturePath) && !ObjectUtils.IsValid(preloadedCustomDissolveTexture))
                preloadedCustomDissolveTexture = Resources.Load<Texture2D>(CustomDissolveTexturePath);

            await actor.TransitionAppearanceAsync(Appearance, Duration, easingType, transitionType, transitionParams, preloadedCustomDissolveTexture);
        }
    }
}
