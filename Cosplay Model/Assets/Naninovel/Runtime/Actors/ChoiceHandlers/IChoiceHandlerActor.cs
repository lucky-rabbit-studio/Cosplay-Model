// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent a choice handler actor on scene.
    /// </summary>
    public interface IChoiceHandlerActor : INovelActor
    {
        /// <summary>
        /// Whether the choice handler is active and forces user to select a choice.
        /// </summary>
        bool IsHandlerActive { get; set; }
        /// <summary>
        /// Currently added options to choose from.
        /// </summary>
        IEnumerable<ChoiceState> Choices { get; }

        /// <summary>
        /// Handler should add an option to choose from.
        /// </summary>
        void AddChoice (ChoiceState choice);
        /// <summary>
        /// Handler should remove a choice option with the provided ID.
        /// </summary>
        void RemoveChoice (string id);
        /// <summary>
        /// Handler should fetch a choice state with the provided ID.
        /// </summary>
        ChoiceState GetChoice (string id);
    } 
}
