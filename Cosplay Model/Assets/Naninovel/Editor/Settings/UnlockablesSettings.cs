// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEditor;

namespace Naninovel
{
    public class UnlockablesSettings : ResourcefulSettings<UnlockablesConfiguration>
    {
        protected override string ResourcesCategoryId => Configuration.LoaderConfiguration.PathPrefix;
        protected override string ResourcesSelectionTooltip => "In novel scripts use `@unlock %name%` to unlock or `@lock %name%` to lock selected unlockable item.";

        [MenuItem("Naninovel/Resources/Unlockables")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
