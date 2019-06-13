// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;

namespace Naninovel
{
    /// <summary>
    /// Arguments associated with the game save and load events invoked by <see cref="StateManager"/>. 
    /// </summary>
    public class GameSaveLoadArgs : EventArgs
    {
        /// <summary>
        /// ID of the save slot the operation is associated with.
        /// </summary>
        public readonly string SlotId;
        /// <summary>
        /// Whether it's a quick save/load operation.
        /// </summary>
        public readonly bool Quick;
        /// <summary>
        /// The map where all the game state data is (will be) de-/serialized from/to.
        /// </summary>
        public readonly GameStateMap StateMap;

        public GameSaveLoadArgs (string slotId, bool quick, GameStateMap stateMap)
        {
            SlotId = slotId;
            Quick = quick;
            StateMap = stateMap;
        }
    }
}