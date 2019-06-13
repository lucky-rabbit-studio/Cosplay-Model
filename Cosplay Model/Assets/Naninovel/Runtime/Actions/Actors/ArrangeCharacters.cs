// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel.Actions
{
    /// <summary>
    /// Arranges specified characters by X-axis.
    /// When no parameters provided, will execute an auto-arrange evenly distributing visible characters by X-axis.
    /// </summary>
    /// <example>
    /// ; Evenly distribute all the visible characters
    /// @arrange
    /// 
    /// ; Place character with ID `Jenna` 15%, `Felix` 50% and `Mia` 85% away 
    /// ; from the left border of the screen.
    /// @arrange Jenna.15,Felix.50,Mia.85
    /// </example>
    [ActionAlias("arrange")]
    public class ArrangeCharacters : NovelAction
    {
        private struct UndoData { public string Id; public Vector3 Position; public CharacterLookDirection LookDirection; }

        /// <summary>
        /// A collection of character ID to scene X-axis position (relative to the left screen border, in percents) named values.
        /// Position 0 relates to the left border and 100 to the right border of the screen; 50 is the center.
        /// </summary>
        [ActionParameter(NamelessParameterAlias, true)]
        public Named<float>[] CharacterPositions { get => GetDynamicParameter<Named<float>[]>(null); set => SetDynamicParameter(value); }

        private List<UndoData> undoData;

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<CharacterManager>();
            undoData = manager.GetAllActors().Select(a => new UndoData { Id = a.Id, Position = a.Position, LookDirection = a.LookDirection }).ToList();

            // When positions are not specified execute auto arrange.
            if (CharacterPositions is null || CharacterPositions.Length == 0)
            {
                await manager.ArrangeCharactersAsync(Duration, EasingType.SmoothStep);
                return;
            }

            var actors = manager.GetAllActors().ToList();
            var arrangeTasks = new List<Task>();

            foreach (var actorPos in CharacterPositions)
            {
                var actor = actors.Find(a => a.Id.EqualsFastIgnoreCase(actorPos.Item1));
                var posX = actorPos.Item2 / 100f; // Implementation is expecting local scene pos, not percents.
                if (actor is null)
                {
                    Debug.LogWarning($"Actor '{actor.Id}' not found while executing arranging task.");
                    continue;
                }
                var newPosX = manager.SceneToWorldSpace(new Vector2(posX, 0)).x;
                var newDir = manager.LookAtOriginDirection(newPosX);
                arrangeTasks.Add(actor.ChangeLookDirectionAsync(newDir, Duration, EasingType.SmoothStep));
                arrangeTasks.Add(actor.ChangePositionXAsync(newPosX, Duration, EasingType.SmoothStep));
            }

            // Sorting by z in order of declaration (first is bottom).
            var declaredActorIds = CharacterPositions.Select(a => a.Item1).ToList();
            declaredActorIds.Reverse();
            for (int i = 0; i < declaredActorIds.Count - 1; i++)
            {
                var currentActor = actors.Find(a => a.Id.EqualsFastIgnoreCase(declaredActorIds[i]));
                var nextActor = actors.Find(a => a.Id.EqualsFastIgnoreCase(declaredActorIds[i + 1]));
                if (currentActor is null || nextActor is null) continue;

                if (currentActor.Position.z > nextActor.Position.z)
                {
                    var lowerZPos = nextActor.Position.z;
                    var higherZPos = currentActor.Position.z;

                    nextActor.ChangePositionZ(higherZPos);
                    currentActor.ChangePositionZ(lowerZPos);
                }
            }

            await Task.WhenAll(arrangeTasks);
        }

        public override Task UndoAsync ()
        {
            if (undoData is null || undoData.Count == 0)
                return Task.CompletedTask;

            var manager = Engine.GetService<CharacterManager>();
            foreach (var data in undoData)
            {
                var actor = manager.GetActor(data.Id);
                if (actor is null)
                {
                    Debug.LogWarning($"Actor `{actor.Id}` not found while undoing `{typeof(ArrangeCharacters).Name}` task.");
                    continue;
                }
                actor.Position = data.Position;
                actor.LookDirection = data.LookDirection;
            }

            undoData = null;
            return Task.CompletedTask;
        }
    }
}
