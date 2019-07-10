// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using Naninovel.Commands;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.FX
{
    /// <summary>
    /// Shakes a <see cref="IActor"/>.
    /// </summary>
    public abstract class ShakeActor : MonoBehaviour, Spawn.IParameterized, Spawn.IAwaitable
    {
        protected string ActorId { get; private set; }
        protected int ShakesCount { get; private set; }
        protected float ShakeDuration { get; private set; }
        protected float DurationVariation { get; private set; }
        protected float ShakeAmplitude { get; private set; }
        protected float AmplitudeVariation { get; private set; }
        protected bool ShakeHorizontally { get; private set; }
        protected bool ShakeVertically { get; private set; }

        [SerializeField] private int defaultShakesCount = 3;
        [SerializeField] private float defaultShakeDuration = .15f;
        [SerializeField] private float defaultDurationVariation = .25f;
        [SerializeField] private float defaultShakeAmplitude = .5f;
        [SerializeField] private float defaultAmplitudeVariation = .5f;
        [SerializeField] private bool defaultShakeHorizontally = false;
        [SerializeField] private bool defaultShakeVertically = true;

        private Vector3 initialPos;
        private Vector3 deltaPos;
        private bool loop;

        public abstract IActor GetActor ();

        public virtual void SetSpawnParameters (string[] parameters)
        {
            ActorId = parameters?.ElementAtOrDefault(0);
            ShakesCount = Mathf.Abs(parameters?.ElementAtOrDefault(1)?.AsInvariantInt() ?? defaultShakesCount);
            ShakeDuration = Mathf.Abs(parameters?.ElementAtOrDefault(2)?.AsInvariantFloat() ?? defaultShakeDuration);
            DurationVariation = Mathf.Clamp01(parameters?.ElementAtOrDefault(3)?.AsInvariantFloat() ?? defaultDurationVariation);
            ShakeAmplitude = Mathf.Abs(parameters?.ElementAtOrDefault(4)?.AsInvariantFloat() ?? defaultShakeAmplitude);
            AmplitudeVariation = Mathf.Clamp01(parameters?.ElementAtOrDefault(5)?.AsInvariantFloat() ?? defaultAmplitudeVariation);
            ShakeHorizontally = bool.Parse(parameters?.ElementAtOrDefault(6) ?? defaultShakeHorizontally.ToString());
            ShakeVertically = bool.Parse(parameters?.ElementAtOrDefault(7) ?? defaultShakeVertically.ToString());

            loop = ShakesCount <= 0;
        }

        public async Task AwaitSpawnAsync ()
        {
            var actor = GetActor();
            if (actor is null)
            {
                Destroy();
                Debug.LogWarning($"Failed to apply `{GetType().Name}` FX to `{ActorId}` actor: actor not found.");
                return;
            }

            initialPos = actor.Position;
            deltaPos = new Vector3(ShakeHorizontally ? ShakeAmplitude : 0, ShakeVertically ? ShakeAmplitude : 0, 0);

            if (loop)
            {
                while (loop && Application.isPlaying)
                    await ShakeSequenceAsync(actor);
            }
            else
            {
                for (int i = 0; i < ShakesCount; i++)
                    await ShakeSequenceAsync(actor);
                Destroy();
            }
        }

        protected virtual async Task ShakeSequenceAsync (IActor actor)
        {
            var amplitude = deltaPos + deltaPos * Random.Range(-AmplitudeVariation, AmplitudeVariation);
            var duration = ShakeDuration + ShakeDuration * Random.Range(-DurationVariation, DurationVariation);

            await actor.ChangePositionAsync(initialPos - amplitude * .5f, duration * .25f, EasingType.SmoothStep);
            await actor.ChangePositionAsync(initialPos + amplitude, duration * .5f, EasingType.SmoothStep);
            await actor.ChangePositionAsync(initialPos, duration * .25f, EasingType.SmoothStep);
        }

        private void Destroy ()
        {
            Engine.GetService<SpawnManager>()?.DestroySpawnedObject(gameObject.name);
        }

        private void OnDestroy ()
        {
            loop = false;
            var actor = GetActor();
            if (actor != null)
                actor.Position = initialPos;
        }
    }
}
