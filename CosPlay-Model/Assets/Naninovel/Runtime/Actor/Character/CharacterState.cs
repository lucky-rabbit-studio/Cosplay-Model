// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of <see cref="ICharacterActor"/>.
    /// </summary>
    [System.Serializable]
    public class CharacterState : ActorState<ICharacterActor>
    {
        public CharacterLookDirection LookDirection;

        public override void ApplyToActor (ICharacterActor actor)
        {
            base.ApplyToActor(actor);
            actor.LookDirection = LookDirection;
        }

        public override void OverwriteFromActor (ICharacterActor actor)
        {
            base.OverwriteFromActor(actor);
            LookDirection = actor.LookDirection;
        }
    }
}
