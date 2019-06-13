﻿// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityCommon;

namespace Naninovel
{
    public class ChoiceHandlersSettings : ActorManagerSettings<ChoiceHandlersConfiguration, IChoiceHandlerActor, ChoiceHandlerMetadata>
    {
        protected override string HelpUri => "guide/choices.html";
        protected override Type ResourcesTypeConstraint => GetTypeConstraint();
        protected override string ResourcesSelectionTooltip => GetTooltip();
        protected override bool AllowMultipleResources => false;
        protected override HashSet<string> LockedActorIds => new HashSet<string> { "ButtonList", "ButtonArea" };

        private Type GetTypeConstraint ()
        {
            switch (EditedMetadata?.Implementation?.GetAfter("."))
            {
                case nameof(UIChoiceHandler): return typeof(UI.ChoiceHandlerPanel);
                default: return null;
            }
        }

        private string GetTooltip ()
        {
            if (EditedActorId == Configuration.DefaultHandlerId)
                return $"Use `@choice \"Choice summary text.\"` in novel scripts to add a choice with this handler.";
            return $"Use `@choice \"Choice summary text.\" handler:{EditedActorId}` in novel scripts to add a choice with this handler.";
        }

        [MenuItem("Naninovel/Resources/Choice Handlers")]
        private static void OpenResourcesWindow () => OpenResourcesWindowImpl();
    }
}
