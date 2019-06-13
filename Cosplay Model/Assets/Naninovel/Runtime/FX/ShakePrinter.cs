// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.FX
{
    /// <summary>
    /// Shakes a <see cref="ITextPrinterActor"/> with provided name or an active one.
    /// </summary>
    public class ShakePrinter : ShakeActor
    {
        public override INovelActor GetActor ()
        {
            var mngr = Engine.GetService<TextPrinterManager>();
            return string.IsNullOrEmpty(ActorId) ? mngr.GetActivePrinter() : mngr.GetActor(ActorId);
        }
    }
}