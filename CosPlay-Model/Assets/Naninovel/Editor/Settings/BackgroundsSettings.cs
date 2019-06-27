// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityCommon;
using UnityEditor;

namespace Naninovel
{
    public class BackgroundsSettings : ActorManagerSettings<BackgroundsConfiguration, IBackgroundActor, BackgroundMetadata>
    {
        protected override string HelpUri => "guide/backgrounds.html";
        protected override Type ResourcesTypeConstraint => GetTypeConstraint();
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override bool AllowMultipleResources => EditedMetadata?.Implementation != typeof(GenericBackground).FullName;
        protected override HashSet<string> LockedActorIds => new HashSet<string> { BackgroundManager.MainActorId };

        private static bool editMainRequested;

        public override void OnGUI (string searchContext)
        {
            if (editMainRequested)
            {
                editMainRequested = false;
                MetadataMapEditor.SelectEditedMetadata(BackgroundManager.MainActorId);
            }

            base.OnGUI(searchContext);
        }

        private Type GetTypeConstraint ()
        {
            switch (EditedMetadata?.Implementation?.GetAfter("."))
            {
                case nameof(SpriteBackground): return typeof(UnityEngine.Texture2D);
                case nameof(GenericBackground): return typeof(BackgroundActorBehaviour);
                case nameof(SceneBackground): return typeof(SceneAsset);
                case nameof(VideoBackground): return typeof(UnityEngine.Video.VideoClip);
                default: return null;
            }
        }

        private string GetTooltip ()
        {
            if (EditedActorId == BackgroundManager.MainActorId && AllowMultipleResources)
                return "Use `@back %name%` in naninovel scripts to show main background with the selected appearance.";
            else if (AllowMultipleResources)
                return $"Use `@back %name% id:{EditedActorId}` in naninovel scripts to show this background with the selected appearance.";
            return $"Use `@back id:{EditedActorId}` in naninovel scripts to show this background.";
        }

        [MenuItem("Naninovel/Resources/Backgrounds")]
        private static void OpenResourcesWindow ()
        {
            // Automatically open main background editor when opened via resources context menu.
            editMainRequested = true;
            OpenResourcesWindowImpl();
        }
    }
}
