// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Commands
{
    public abstract class ModifyOrthoActor<TActor, TState, TManager> : ModifyActor<TActor, TState, TManager>
        where TActor : IActor
        where TState : ActorState<TActor>, new()
        where TManager : OrthoActorManager<TActor, TState>
    {
        /// <summary>
        /// Position (relative to the screen borders, in percents) to set for the modified actor.
        /// Position is described as follows: `0,0` is the bottom left, `50,50` is the center and `100,100` is the top right corner of the screen.
        /// </summary>
        [CommandParameter("pos", true)]
        public virtual float?[] ScenePosition { get => GetDynamicParameter<float?[]>(null); set => SetDynamicParameter(value); }

        [CommandParameter(optional: true)] // Allows using scale=x for uniform scaling.
        public override float?[] Scale { get => AttemptUniformScale(); set => base.Scale = value; }
        [CommandParameter(optional: true)] // Allows using local scene position to set world position of the actor.
        public override float?[] Position { get => AttemptScenePosition(); set => base.Position = value; }

        private float?[] uniformScale = new float?[3];
        private float?[] worldPosition = new float?[3];

        protected override Task ApplyPositionModificationAsync (TActor actor, EasingType easingType)
        {
            // In ortho mode, there is no point in animating z position.
            if (Position != null) actor.ChangePositionZ(Position.ElementAtOrDefault(2) ?? actor.Position.z);

            return base.ApplyPositionModificationAsync(actor, easingType);
        }

        private float?[] AttemptScenePosition ()
        {
            if (ScenePosition is null) return base.Position;

            worldPosition[0] = ScenePosition.ElementAtOrDefault(0) != null ? ActorManager?.SceneToWorldSpace(new Vector2(ScenePosition[0].Value / 100f, 0)).x : null;
            worldPosition[1] = ScenePosition.ElementAtOrDefault(1) != null ? ActorManager?.SceneToWorldSpace(new Vector2(0, ScenePosition[1].Value / 100f)).y : null;
            worldPosition[2] = ScenePosition.ElementAtOrDefault(2);

            return worldPosition;
        }

        private float?[] AttemptUniformScale ()
        {
            var scale = base.Scale;

            if (scale != null && scale.Length == 1 && scale[0].HasValue)
            {
                var scaleX = scale[0].Value;
                uniformScale[0] = scaleX;
                uniformScale[1] = scaleX;
                uniformScale[2] = scaleX;
                return uniformScale;
            }

            return scale;
        }
    } 
}
