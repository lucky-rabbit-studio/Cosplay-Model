// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Actions
{
    /// <summary>
    /// Hides (removes) all the visible characters on scene.
    /// </summary>
    /// <example>
    /// @hideChars
    /// </example>
    [ActionAlias("hideChars")]
    public class HideAllCharacters : NovelAction
    {
        private struct UndoData { public bool WasVisible; public string ActorId; }

        private List<UndoData> undoData;

        public override async Task ExecuteAsync ()
        {
            var manager = Engine.GetService<CharacterManager>();

            undoData = new List<UndoData>();
            undoData.AddRange(manager.GetAllActors().Select(a => new UndoData { ActorId = a.Id, WasVisible = a.IsVisible }));

            await Task.WhenAll(manager.GetAllActors().Select(a => a.ChangeVisibilityAsync(false, Duration)));
        }

        public override Task UndoAsync ()
        {
            if (undoData is null || undoData.Count == 0) return Task.CompletedTask;

            var manager = Engine.GetService<CharacterManager>();
            foreach (var data in undoData)
                manager.GetActor(data.ActorId).IsVisible = data.WasVisible;

            undoData = null;
            return Task.CompletedTask;
        }
    }
}
