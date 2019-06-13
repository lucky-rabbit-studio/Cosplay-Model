// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="INovelActor"/>.
    /// </summary>
    [System.Serializable]
    public abstract class NovelActorState
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

        public abstract void ApplyToActor (INovelActor actor);
        public abstract void OverwriteFromActor (INovelActor actor);
    }

    public abstract class NovelActorState<TActor> : NovelActorState
        where TActor : INovelActor
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

        public override void ApplyToActor (INovelActor actor) => ApplyToActor(actor);
        public override void OverwriteFromActor (INovelActor actor) => OverwriteFromActor(actor);
    }
}
