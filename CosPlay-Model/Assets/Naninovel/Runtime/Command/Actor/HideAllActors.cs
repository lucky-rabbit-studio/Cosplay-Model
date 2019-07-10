// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Naninovel.Commands
{
    /// <summary>
    /// Hides (removes) all the actors (eg characters, backgrounds, text printers, choice handlers, etc) on scene.
    /// </summary>
    /// <example>
    /// @hideAll
    /// </example>
    [CommandAlias("hideAll")]
    public class HideAllActors : Command
    {
        private struct UndoData { public bool WasVisible; public IActorManager Manager; public string ActorId; }

        private List<UndoData> undoData;

        public override async Task ExecuteAsync ()
        {
            var managers = Engine.GetAllServices<IActorManager>();

            undoData = new List<UndoData>();
            foreach (var manager in managers)
                undoData.AddRange(manager.GetAllActors().Select(a => new UndoData { Manager = manager, ActorId = a.Id, WasVisible = a.IsVisible }));

            await Task.WhenAll(managers.SelectMany(m => m.GetAllActors()).Select(a => a.ChangeVisibilityAsync(false, Duration)));
        }

        public override Task UndoAsync ()
        {
            if (undoData is null || undoData.Count == 0) return Task.CompletedTask;

            foreach (var data in undoData)
                data.Manager.GetActor(data.ActorId).IsVisible = data.WasVisible;

            undoData = null;
            return Task.CompletedTask;
        }
    } 
}
