// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Actions;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.FX
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Rain : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable, DestroySpawned.IParameterized, DestroySpawned.IAwaitable
    {
        protected float Intensity { get; private set; }
        protected float FadeInTime { get; private set; }
        protected float FadeOutTime { get; private set; }

        [SerializeField] private float defaultIntensity = 500f;
        [SerializeField] private float defaultFadeInTime = 5f;
        [SerializeField] private float defaultFadeOutTime = 5f;
        [SerializeField] private float defaultVelocityX = 1f;
        [SerializeField] private float defaultVelocityY = 1f;

        private ParticleSystem particles;
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.VelocityOverLifetimeModule velocityModule;
        private Tweener<FloatTween> intensityTweener;

        public virtual void SetSpawnParameters (string[] parameters)
        {
            Intensity = parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultIntensity;
            FadeInTime = Mathf.Abs(parameters?.ElementAtOrDefault(1)?.AsInvariantFloat() ?? defaultFadeInTime);
            velocityModule.xMultiplier *= parameters?.ElementAtOrDefault(2)?.AsInvariantFloat() ?? defaultVelocityX;
            velocityModule.yMultiplier *= parameters?.ElementAtOrDefault(3)?.AsInvariantFloat() ?? defaultVelocityY;
        }

        public async Task AwaitSpawnAsync ()
        {
            if (intensityTweener.IsRunning)
                intensityTweener.CompleteInstantly();

            var tween = new FloatTween(emissionModule.rateOverTimeMultiplier, Intensity, FadeInTime, SetIntensity);
            await intensityTweener.RunAsync(tween);
        }

        public void SetDestroyParameters (string[] parameters)
        {
            FadeOutTime = Mathf.Abs(parameters?.ElementAtOrDefault(0)?.AsInvariantFloat() ?? defaultFadeOutTime);
        }

        public async Task AwaitDestroyAsync ()
        {
            if (intensityTweener.IsRunning)
                intensityTweener.CompleteInstantly();

            var tween = new FloatTween(emissionModule.rateOverTimeMultiplier, 0, FadeOutTime, SetIntensity);
            await intensityTweener.RunAsync(tween);
        }

        private void Awake ()
        {
            particles = GetComponent<ParticleSystem>();
            emissionModule = particles.emission;
            velocityModule = particles.velocityOverLifetime;
            intensityTweener = new Tweener<FloatTween>(this);

            SetIntensity(0);
        }

        private void Start ()
        {
            // Position before the first background.
            var backgroundMngr = Engine.GetService<BackgroundManager>();
            transform.position = new Vector3(0, 0, backgroundMngr.ZOffset - 1);
        }

        private void SetIntensity (float value) => emissionModule.rateOverTimeMultiplier = value;
    }
}
