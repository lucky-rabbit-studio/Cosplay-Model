// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IActor"/> implementation using <see cref="MonoBehaviour"/> to represent an actor.
    /// </summary>
    public abstract class MonoBehaviourActor : IActor, IDisposable
    {
        public virtual string Id { get; }
        public abstract string Appearance { get; set; }
        public abstract bool IsVisible { get; set; }
        public virtual Vector3 Position
        {
            get => position;
            set { CompletePositionTween(); position = value; SetBehaviourPosition(value); }
        }
        public virtual Quaternion Rotation
        {
            get => rotation;
            set { CompleteRotationTween(); rotation = value; SetBehaviourRotation(value); }
        }
        public virtual Vector3 Scale
        {
            get => scale;
            set { CompleteScaleTween(); scale = value; SetBehaviourScale(value); }
        }
        public virtual Color TintColor
        {
            get => tintColor;
            set { CompleteTintColorTween(); tintColor = value; SetBehaviourTintColor(value); }
        }

        protected virtual ProxyBehaviour ActorBehaviour { get; }
        protected virtual GameObject GameObject { get; private set; }
        protected virtual Transform Transform => GameObject.transform;

        private Vector3 position = Vector3.zero;
        private Vector3 scale = Vector3.one;
        private Quaternion rotation = Quaternion.identity;
        private Color tintColor = Color.white;
        private Tweener<VectorTween> positionTweener, rotationTweener, scaleTweener;
        private Tweener<ColorTween> tintColorTweener;

        public MonoBehaviourActor (string id, ActorMetadata metadata)
        {
            Id = id;
            GameObject = Engine.CreateObject(id);
            ActorBehaviour = GameObject.AddComponent<ProxyBehaviour>();
            positionTweener = new Tweener<VectorTween>(ActorBehaviour);
            rotationTweener = new Tweener<VectorTween>(ActorBehaviour);
            scaleTweener = new Tweener<VectorTween>(ActorBehaviour);
            tintColorTweener = new Tweener<ColorTween>(ActorBehaviour);
        }

        public virtual Task InitializeAsync () => Task.CompletedTask; 

        public abstract Task ChangeAppearanceAsync (string appearance, float duration, EasingType easingType = default);

        public abstract Task ChangeVisibilityAsync (bool isVisible, float duration, EasingType easingType = default);

        public virtual async Task ChangePositionAsync (Vector3 position, float duration, EasingType easingType = default)
        {
            CompletePositionTween();
            this.position = position;

            var tween = new VectorTween(GetBehaviourPosition(), position, duration, SetBehaviourPosition, false, easingType);
            await positionTweener.RunAsync(tween);
        }

        public virtual async Task ChangeRotationAsync (Quaternion rotation, float duration, EasingType easingType = default)
        {
            CompleteRotationTween();
            this.rotation = rotation;

            var tween = new VectorTween(GetBehaviourRotation().ClampedEulerAngles(), rotation.ClampedEulerAngles(), duration, SetBehaviourRotation, false, easingType);
            await rotationTweener.RunAsync(tween);
        }

        public virtual async Task ChangeScaleAsync (Vector3 scale, float duration, EasingType easingType = default)
        {
            CompleteScaleTween();
            this.scale = scale;

            var tween = new VectorTween(GetBehaviourScale(), scale, duration, SetBehaviourScale, false, easingType);
            await scaleTweener.RunAsync(tween);
        }

        public virtual async Task ChangeTintColorAsync (Color tintColor, float duration, EasingType easingType = default)
        {
            CompleteTintColorTween();
            this.tintColor = tintColor;

            var tween = new ColorTween(GetBehaviourTintColor(), tintColor, ColorTweenMode.All, duration, SetBehaviourTintColor, false, easingType);
            await tintColorTweener.RunAsync(tween);
        }

        public virtual Task HoldResourcesAsync (object holder, string appearance) => Task.CompletedTask;

        public virtual void ReleaseResources (object holder, string appearance) { }

        public virtual void Dispose () => UnityEngine.Object.Destroy(GameObject);

        protected virtual Vector3 GetBehaviourPosition () => Transform.position;
        protected virtual void SetBehaviourPosition (Vector3 position) => Transform.position = position;
        protected virtual Quaternion GetBehaviourRotation () => Transform.rotation;
        protected virtual void SetBehaviourRotation (Quaternion rotation) => Transform.rotation = rotation;
        protected virtual void SetBehaviourRotation (Vector3 rotation) => SetBehaviourRotation(Quaternion.Euler(rotation));
        protected virtual Vector3 GetBehaviourScale () => Transform.localScale;
        protected virtual void SetBehaviourScale (Vector3 scale) => Transform.localScale = scale;
        protected abstract Color GetBehaviourTintColor ();
        protected abstract void SetBehaviourTintColor (Color tintColor);

        private void CompletePositionTween ()
        {
            if (positionTweener.IsRunning)
                positionTweener.CompleteInstantly();
        }

        private void CompleteRotationTween ()
        {
            if (rotationTweener.IsRunning)
                rotationTweener.CompleteInstantly();
        }

        private void CompleteScaleTween ()
        {
            if (scaleTweener.IsRunning)
                scaleTweener.CompleteInstantly();
        }

        private void CompleteTintColorTween ()
        {
            if (tintColorTweener.IsRunning)
                tintColorTweener.CompleteInstantly();
        }
    }
}
