// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="IActor"/>.
    /// </summary>
    [System.Serializable]
    public abstract class ActorState
    {
        public string Id;
        public string Appearance;
        public bool IsVisible;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public Color TintColor;

        public void OverwriteFromJson (string json) => JsonUtility.FromJsonOverwrite(json, this);
        public string ToJson () => JsonUtility.ToJson(this);

        public abstract void ApplyToActor (IActor actor);
        public abstract void OverwriteFromActor (IActor actor);
    }

    public abstract class ActorState<TActor> : ActorState
        where TActor : IActor
    {
        public virtual void ApplyToActor (TActor actor)
        {
            actor.Appearance = Appearance;
            actor.IsVisible = IsVisible;
            actor.Position = Position;
            actor.Rotation = Rotation;
            actor.Scale = Scale;
            actor.TintColor = TintColor;
        }

        public virtual void OverwriteFromActor (TActor actor)
        {
            Id = actor.Id;
            Appearance = actor.Appearance;
            IsVisible = actor.IsVisible;
            Position = actor.Position;
            Rotation = actor.Rotation;
            Scale = actor.Scale;
            TintColor = actor.TintColor;
        }

        public override void ApplyToActor (IActor actor) => ApplyToActor(actor);
        public override void OverwriteFromActor (IActor actor) => OverwriteFromActor(actor);
    }
}
