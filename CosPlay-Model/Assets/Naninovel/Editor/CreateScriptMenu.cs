// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public static class CreateScriptMenu
    {
        [MenuItem("Assets/Create/Naninovel Script", priority = 81)]
        private static void CreateNaninovelScript ()
        {
            ProjectWindowUtil.CreateAssetWithContent("NewScript.nani", $"{Environment.NewLine}@stop");
        }
    }
}
