// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a <see cref="IChoiceHandlerActor"/>.
    /// </summary>
    [System.Serializable]
    public class ChoiceHandlerState : NovelActorState<IChoiceHandlerActor>
    {
        public bool IsHandlerActive = false;
        public List<ChoiceState> Choices = new List<ChoiceState>();

        public override void ApplyToActor (IChoiceHandlerActor actor)
        {
            base.ApplyToActor(actor);
            actor.IsHandlerActive = IsHandlerActive;
            foreach (var choice in actor.Choices)
                if (!Choices.Contains(choice))
                    actor.RemoveChoice(choice.Id);
            foreach (var choice in Choices)
                if (!actor.Choices.Contains(choice))
                    actor.AddChoice(choice);
        }

        public override void OverwriteFromActor (IChoiceHandlerActor actor)
        {
            base.OverwriteFromActor(actor);
            IsHandlerActive = actor.IsHandlerActive;
            Choices.Clear();
            foreach (var choice in actor.Choices)
                Choices.Add(choice);
        }
    }
}
