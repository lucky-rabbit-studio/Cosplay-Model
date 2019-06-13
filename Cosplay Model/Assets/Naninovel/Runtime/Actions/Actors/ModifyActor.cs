// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    public abstract class ModifyActor<TActor, TState, TManager> : NovelAction, NovelAction.IPreloadable 
        where TActor : INovelActor
        where TState : NovelActorState<TActor>, new()
        where TManager : NovelActorManager<TActor, TState>
    {
        private struct UndoData { public bool Executed; public TState State; }

        /// <summary>
        /// ID of the actor to modify.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual string Id { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Appearance to set for the modified actor.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual string Appearance { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Visibility status to set for the modified actor.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual bool? IsVisible { get => GetDynamicParameter<bool?>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Position (in world space) to set for the modified actor.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual float?[] Position { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Rotation to set for the modified actor.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual float?[] Rotation { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Scale to set for the modified actor.
        /// </summary>
        [ActionParameter(optional: true)]
        public virtual float?[] Scale { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Tint color to set for the modified actor.
        /// <br/><br/>
        /// Strings that begin with `#` will be parsed as hexadecimal in the following way: 
        /// `#RGB` (becomes RRGGBB), `#RRGGBB`, `#RGBA` (becomes RRGGBBAA), `#RRGGBBAA`; when alpha is not specified will default to FF.
        /// <br/><br/>
        /// Strings that do not begin with `#` will be parsed as literal colors, with the following supported:
        /// red, cyan, blue, darkblue, lightblue, purple, yellow, lime, fuchsia, white, silver, grey, black, orange, brown, maroon, green, olive, navy, teal, aqua, magenta.
        /// </summary>
        [ActionParameter("tint", true)]
        public virtual string TintColor { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }
        /// <summary>
        /// Name of the easing function to use for the modification.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the actor's manager configuration settings.
        /// </summary>
        [ActionParameter("easing", true)]
        public virtual string EasingTypeName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        protected virtual TManager ActorManager => actorMngrCache ?? (actorMngrCache = Engine.GetService<TManager>());

        private UndoData undoData;
        private TManager actorMngrCache;

        public virtual async Task PreloadResourcesAsync ()
        {
            if (ActorManager is null || string.IsNullOrWhiteSpace(Id)) return;
            var actor = await ActorManager.GetOrAddActorAsync(Id);
            await actor.PreloadResourcesAsync(Appearance);
        }

        public virtual async Task UnloadResourcesAsync ()
        {
            if (ActorManager is null || string.IsNullOrWhiteSpace(Id)) return;
            if (ActorManager.ActorExists(Id)) await ActorManager.GetActor(Id).UnloadResourcesAsync(Appearance);
        }

        public override async Task ExecuteAsync ()
        {
            if (ActorManager is null)
            {
                Debug.LogError("Can't resolve actors manager.");
                return;
            }

            if (string.IsNullOrEmpty(Id))
            {
                Debug.LogError("Actor ID was not provided.");
                return;
            }

            var actor = await ActorManager.GetOrAddActorAsync(Id);

            undoData.Executed = true;
            undoData.State = ActorManager.GetActorState(actor.Id);

            var easingType = ActorManager.DefaultEasingType;
            if (!string.IsNullOrEmpty(EasingTypeName) && !Enum.TryParse(EasingTypeName, true, out easingType))
                Debug.LogWarning($"Failed to parse `{EasingTypeName}` easing.");
            await ApplyModificationsAsync(actor, easingType);
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            var actor = ActorManager.GetActor(undoData.State?.Id);
            if (actor == null)
            {
                Debug.LogWarning($"Actor `{undoData.State?.Id}` not found while undoing `{typeof(ModifyActor<TActor, TState, TManager>).Name}` task.");
                return Task.CompletedTask;
            }

            undoData.State.ApplyToActor(actor);

            undoData = default;
            return Task.CompletedTask;
        }

        protected virtual async Task ApplyModificationsAsync (TActor actor, EasingType easingType)
        {
            // When visibility is not explicitly specified assume user would like to show the actor anyway.
            if (!IsVisible.HasValue) IsVisible = true;

            await Task.WhenAll(
                    ApplyAppearanceModificationAsync(actor, easingType),
                    ApplyVisibilityModificationAsync(actor, easingType),
                    ApplyPositionModificationAsync(actor, easingType),
                    ApplyRotationModificationAsync(actor, easingType),
                    ApplyScaleModificationAsync(actor, easingType),
                    ApplyTintColorModificationAsync(actor, easingType)
                );
        }

        protected virtual async Task ApplyAppearanceModificationAsync (TActor actor, EasingType easingType)
        {
            if (string.IsNullOrWhiteSpace(Appearance)) return;
            await actor.ChangeAppearanceAsync(Appearance, Duration, easingType);
        }

        protected virtual async Task ApplyVisibilityModificationAsync (TActor actor, EasingType easingType)
        {
            if (IsVisible is null) return;
            await actor.ChangeVisibilityAsync(IsVisible.Value, Duration, easingType);
        }

        protected virtual async Task ApplyPositionModificationAsync (TActor actor, EasingType easingType)
        {
            if (Position is null) return;
            await actor.ChangePositionAsync(new Vector3(
                    Position.ElementAtOrDefault(0) ?? actor.Position.x,
                    Position.ElementAtOrDefault(1) ?? actor.Position.y,
                    Position.ElementAtOrDefault(2) ?? actor.Position.z), Duration, easingType);
        }

        protected virtual async Task ApplyRotationModificationAsync (TActor actor, EasingType easingType)
        {
            if (Rotation is null) return;
            await actor.ChangeRotationAsync(Quaternion.Euler(
                    Rotation.ElementAtOrDefault(0) ?? actor.Rotation.eulerAngles.x,
                    Rotation.ElementAtOrDefault(1) ?? actor.Rotation.eulerAngles.y,
                    Rotation.ElementAtOrDefault(2) ?? actor.Rotation.eulerAngles.z), Duration, easingType);
        }

        protected virtual async Task ApplyScaleModificationAsync (TActor actor, EasingType easingType)
        {
            if (Scale is null) return;
            await actor.ChangeScaleAsync(new Vector3(
                    Scale.ElementAtOrDefault(0) ?? actor.Scale.x,
                    Scale.ElementAtOrDefault(1) ?? actor.Scale.y,
                    Scale.ElementAtOrDefault(2) ?? actor.Scale.z), Duration, easingType);
        }

        protected virtual async Task ApplyTintColorModificationAsync (TActor actor, EasingType easingType)
        {
            if (TintColor is null) return;
            if (!ColorUtility.TryParseHtmlString(TintColor, out var color))
            {
                Debug.LogError($"Failed to parse `{TintColor}` color to apply tint modification for `{actor.Id}` actor. See the API docs for supported color formats.");
                return;
            }
            await actor.ChangeTintColorAsync(color, Duration, easingType);
        }
    } 
}
