// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Stops [special effect](/guide/special-effects.md) started with [`@fx`](/api/#fx) command by destroying spawned object of the corresponding FX prefab.
    /// </summary>
    /// <example>
    /// ; Given a "Rain" FX was started with "@fx" command
    /// @stopfx Rain
    /// </example>
    [CommandAlias("stopFx")]
    public class DestroySpawnedFx : DestroySpawned
    {
        protected override string FullPath => "Naninovel/FX/" + Path;
    }
}
