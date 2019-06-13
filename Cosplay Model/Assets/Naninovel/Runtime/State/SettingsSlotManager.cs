// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.IO;

namespace Naninovel
{
    public class SettingsSlotManager : StateSlotManager<SettingsStateMap>
    {
        private string defaultSlotId;

        public SettingsSlotManager (string saveFolderName, string defaultSlotId) : base(saveFolderName)
        {
            this.defaultSlotId = defaultSlotId;
        }

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            return Directory.GetFiles(SaveDataPath, $"{defaultSlotId}.json", SearchOption.TopDirectoryOnly).Length > 0;
        }
    }
}
