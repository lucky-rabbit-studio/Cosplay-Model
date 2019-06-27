// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel
{
    /// <summary>
    /// Implementation represents an <see cref="Engine"/> service.
    /// </summary>
    public interface IEngineService 
    {
        Task InitializeServiceAsync ();
        void ResetService ();
        void DestroyService ();
    }
}
