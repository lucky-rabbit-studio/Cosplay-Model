// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel.Actions
{
    /// <summary>
    /// Sets an [unlockable item](/guide/unlockable-items.md) with the provided ID to `unlocked` state.
    /// </summary>
    /// <remarks>
    /// The unlocked state of the items is stored in [global scope](/guide/state-management.md#global-state).<br/>
    /// In case item with the provided ID is not registered in the global state map, 
    /// the corresponding record will automatically be added.
    /// </remarks>
    /// <example>
    /// @unlock CG/FightScene1
    /// </example>
    public class Unlock : NovelAction
    {
        private struct UndoData { public bool Executed; public string Id; public Dictionary<string, bool> ItemsMap; }

        /// <summary>
        /// ID of the unlockable item. Use `all` to unlock all the registered unlockable items. 
        /// </summary>
        [ActionParameter(NamelessParameterAlias)]
        public string Id { get => GetDynamicParameter<string>(null); set => SetDynamicParameter(value); }

        private UndoData undoData;

        public override async Task ExecuteAsync ()
        {
            var unlockableManager = Engine.GetService<UnlockableManager>();

            undoData.Executed = true;
            undoData.Id = Id;
            undoData.ItemsMap = unlockableManager.GetAllItems();

            if (Id.EqualsFastIgnoreCase("all")) unlockableManager.UnlockAllItems();
            else unlockableManager.UnlockItem(Id);

            await Engine.GetService<StateManager>().SaveGlobalStateAsync();
        }

        public override async Task UndoAsync ()
        {
            if (!undoData.Executed) return;

            var unlockableManager = Engine.GetService<UnlockableManager>();
            if (undoData.Id.EqualsFastIgnoreCase("all"))
                foreach (var kv in undoData.ItemsMap)
                    unlockableManager.SetItemUnlocked(kv.Key, kv.Value);
            else unlockableManager.LockItem(undoData.Id);

            await Engine.GetService<StateManager>().SaveGlobalStateAsync();
        }
    } 
}
