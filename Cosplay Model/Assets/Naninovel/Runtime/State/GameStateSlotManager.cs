// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityCommon;

namespace Naninovel
{
    public class GameStateSlotManager : StateSlotManager<GameStateMap>
    {
        public int SaveSlotLimit { get; }
        public int QuickSaveSlotLimit { get; }

        private string saveSlotMask, quickSaveSlotMask, savePattern, quickSavePattern;

        public GameStateSlotManager (string saveFolderName, string saveSlotMask, string quickSaveSlotMask, int saveSlotLimit, int quickSaveSlotLimit) 
            : base(saveFolderName)
        {
            this.saveSlotMask = saveSlotMask;
            this.quickSaveSlotMask = quickSaveSlotMask;
            SaveSlotLimit = saveSlotLimit;
            QuickSaveSlotLimit = quickSaveSlotLimit;
            savePattern = string.Format(saveSlotMask, "*") + ".json"; 
            quickSavePattern = string.Format(quickSaveSlotMask, "*") + ".json";
        }

        public override bool AnySaveExists ()
        {
            if (!Directory.Exists(SaveDataPath)) return false;
            var saveExists = Directory.GetFiles(SaveDataPath, savePattern, SearchOption.TopDirectoryOnly).Length > 0;
            var qSaveExists = Directory.GetFiles(SaveDataPath, quickSavePattern, SearchOption.TopDirectoryOnly).Length > 0;
            return saveExists || qSaveExists;
        }

        public string IndexToSaveSlotId (int index) => string.Format(saveSlotMask, index);

        public string IndexToQuickSaveSlotId (int index) => string.Format(quickSaveSlotMask, index);

        /// <summary>
        /// Slots are provided in [slotId]->[state] map format; null state represents an `empty` slot.
        /// </summary>
        public async Task<IDictionary<string, GameStateMap>> LoadAllSaveSlotsAsync ()
        {
            var result = new Dictionary<string, GameStateMap>();
            if (!Directory.Exists(SaveDataPath)) return result;
            for (int i = 1; i <= SaveSlotLimit; i++)
            {
                var slotId = IndexToSaveSlotId(i);
                var state = SaveSlotExists(slotId) ? await LoadAsync(slotId) as GameStateMap : null;
                result.Add(slotId, state);
            }
            return result;
        }

        /// <summary>
        /// Slots are provided in [slotId]->[state] map format; null state represents an `empty` slot.
        /// </summary>
        public async Task<IDictionary<string, GameStateMap>> LoadAllQuickSaveSlotsAsync ()
        {
            var result = new Dictionary<string, GameStateMap>();
            if (!Directory.Exists(SaveDataPath)) return result;
            for (int i = 1; i <= QuickSaveSlotLimit; i++)
            {
                var slotId = IndexToQuickSaveSlotId(i);
                var state = SaveSlotExists(slotId) ? await LoadAsync(slotId) as GameStateMap : null;
                result.Add(slotId, state);
            }
            return result;
        }

        /// <summary>
        /// Frees first quick save slot by shifting existing ones by one.
        /// Will delete the last slot in case it's out of the limit.
        /// </summary>
        public void ShiftQuickSaveSlots ()
        {
            for (int i = QuickSaveSlotLimit; i > 0; i--)
            {
                var curSlotId = IndexToQuickSaveSlotId(i);
                var prevSlotId = IndexToQuickSaveSlotId(i + 1);
                if (!SaveSlotExists(curSlotId)) continue;

                // Shifting file names.
                var curFilePath = SlotIdToFilePath(curSlotId);
                var prevFilePath = SlotIdToFilePath(prevSlotId);
                File.Delete(prevFilePath);
                File.Move(curFilePath, prevFilePath);
            }
            var outOfLimitSlotId = IndexToQuickSaveSlotId(QuickSaveSlotLimit + 1);
            if (SaveSlotExists(outOfLimitSlotId)) File.Delete(SlotIdToFilePath(outOfLimitSlotId));
            IOUtils.WebGLSyncFs();
        }
    }
}
