// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.Actions
{
    /// <summary>
    /// Stops [special effect](/guide/special-effects.md) started with [`@fx`](/api/#fx) action by destroying spawned object of the corresponding FX prefab.
    /// </summary>
    /// <example>
    /// ; Given a "Rain" FX was started with "@fx" action
    /// @stopfx Rain
    /// </example>
    [ActionAlias("stopFx")]
    public class DestroySpawnedFx : DestroySpawned
    {
        protected override string FullPath => "Naninovel/FX/" + Path;
    }
}
