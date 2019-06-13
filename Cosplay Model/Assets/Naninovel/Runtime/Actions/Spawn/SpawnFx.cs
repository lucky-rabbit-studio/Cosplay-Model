// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.Actions
{
    /// <summary>
    /// Spawns a [special effect](/guide/special-effects.md) prefab stored in `./Resources/Naninovel/FX` resources folder.
    /// </summary>
    /// <example>
    /// ; Shakes an active text printer
    /// @fx ShakePrinter
    /// 
    /// ; Applies a glitch effect to the camera
    /// @fx GlitchCamera
    /// </example>
    [ActionAlias("fx")]
    public class SpawnFx : Spawn
    {
        protected override string FullPath => "Naninovel/FX/" + Path;
    }
}
