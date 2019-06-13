// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using UnityEditor;

namespace Naninovel
{
    public static class CreateScriptMenu
    {
        [MenuItem("Assets/Create/Novel Script", priority = 81)]
        private static void CreateNovelScript ()
        {
            ProjectWindowUtil.CreateAssetWithContent("NewScript.txt", $"{Environment.NewLine}@stop");
        }
    }
}
