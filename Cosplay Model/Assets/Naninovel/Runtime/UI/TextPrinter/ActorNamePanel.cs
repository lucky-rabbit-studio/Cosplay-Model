// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public abstract class ActorNamePanel : MonoBehaviour
    {
        public abstract string Text { get; set; }
        public abstract Color TextColor { get; set; }
    }
}
