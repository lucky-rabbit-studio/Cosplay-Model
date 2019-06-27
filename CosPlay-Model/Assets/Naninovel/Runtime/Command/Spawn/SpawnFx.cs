// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.Commands
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
    [CommandAlias("fx")]
    public class SpawnFx : Spawn
    {
        protected override string FullPath => "Naninovel/FX/" + Path;
    }
}
