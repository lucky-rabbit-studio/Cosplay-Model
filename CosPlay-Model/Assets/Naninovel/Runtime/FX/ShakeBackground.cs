// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.FX
{
    /// <summary>
    /// Shakes a <see cref="IBackgroundActor"/> or the main one.
    /// </summary>
    public class ShakeBackground : ShakeActor
    {
        public override IActor GetActor ()
        {
            var mngr = Engine.GetService<BackgroundManager>();
            return string.IsNullOrEmpty(ActorId) ? mngr.GetActor(BackgroundManager.MainActorId) : mngr.GetActor(ActorId);
        }
    }
}
