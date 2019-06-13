// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Hides (removes from scene) an actor with provided ID.
    /// </summary>
    /// <example>
    /// ; Given an actor (eg, character, background, text printer, etc) with ID `SomeActor`
    /// ; is currently visible on scene, the following action will hide it.
    /// @hide SomeActor
    /// </example>
    [ActionAlias("hide")]
    public class HideActor : NovelAction
    {
        private struct UndoData { public bool Executed, WasVisible; public NovelActorManager Manager; public string ActorId; }

        /// <summary>
        /// ID of the actor to hide.
        /// </summary>
        [ActionParameter(alias: NamelessParameterAlias)]
        public string ActorName { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<NovelActorManager>(c => c.ActorExists(ActorName));

            if (manager is null)
            {
                Debug.LogError($"Can't find manager with `{ActorName}` actor.");
                return;
            }

            var actor = manager.GetActor(ActorName);

            undoData.Executed = true;
            undoData.Manager = manager;
            undoData.ActorId = actor.Id;
            undoData.WasVisible = actor.IsVisible;

            await actor.ChangeVisibilityAsync(false, Duration);
        }

        public override Task UndoAsync ()
        {
            if (!undoData.Executed) return Task.CompletedTask;

            undoData.Manager.GetActor(undoData.ActorId).IsVisible = undoData.WasVisible;

            undoData = default;
            return Task.CompletedTask;
        }
    } 
}
