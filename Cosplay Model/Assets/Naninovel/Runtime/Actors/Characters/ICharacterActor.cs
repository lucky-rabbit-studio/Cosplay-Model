// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a character actor on scene.
    /// </summary>
    public interface ICharacterActor : INovelActor
    {
        /// <summary>
        /// Look direction of the actor.
        /// </summary>
        CharacterLookDirection LookDirection { get; set; }

        /// <summary>
        /// Changes character look direction over specified time.
        /// </summary>
        Task ChangeLookDirectionAsync (CharacterLookDirection lookDirection, float duration, EasingType easingType = default);
    } 
}
