// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Linq;

namespace Naninovel.FX
{
    /// <summary>
    /// Shakes a <see cref="ICharacterActor"/> with provided name or a random visible one.
    /// </summary>
    public class ShakeCharacter : ShakeActor
    {
        public override INovelActor GetActor ()
        {
            var mngr = Engine.GetService<CharacterManager>();
            return string.IsNullOrEmpty(ActorId) ? mngr.GetAllActors().FirstOrDefault(a => a.IsVisible) : mngr.GetActor(ActorId);
        }
    }
}
