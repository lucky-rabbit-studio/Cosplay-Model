// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Linq;
using UnityCommon;

namespace Naninovel.UI
{
    public class GameStateSlotsGrid : ScriptableGrid<GameStateSlot>
    {
        public DateTime? LastSaveDateTime => SlotsMap?.Values?.Max(s => s?.State?.SaveDateTime);
    }
}
