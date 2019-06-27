// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityCommon;

namespace Naninovel
{
    public abstract class StateSlotManager<TData> : SaveSlotManager<TData> where TData : StateMap, new()
    {
        protected override string SaveDataPath => $"{GameDataPath}/{saveFolderName}";

        private string saveFolderName;

        public StateSlotManager (string saveFolderName)
        {
            this.saveFolderName = saveFolderName;
        }
    }
}
