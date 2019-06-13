// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.UI
{
    public class SaveLoadMenuManagedText : ManagedTextUITextSetter
    {
        [ManagedText("UISaveLoadMenu")]
        public readonly static string NavigationQuickLoadLabel = "Q.LOAD";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string NavigationLoadLabel = "LOAD";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string NavigationSaveLabel = "SAVE";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string NavigationReturnLabel = "RETURN";

        [ManagedText("UISaveLoadMenu")]
        public readonly static string QuickLoadTitleLabel = "QUICK LOAD";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string LoadTitleLabel = "LOAD GAME";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string SaveTitleLabel = "SAVE GAME";

        [ManagedText("UISaveLoadMenu")]
        public readonly static string OverwriteSaveSlotMessage = "Are you sure you want to overwrite save slot?";
        [ManagedText("UISaveLoadMenu")]
        public readonly static string DeleteSaveSlotMessage = "Are you sure you want to delete save slot?";
    }
}
