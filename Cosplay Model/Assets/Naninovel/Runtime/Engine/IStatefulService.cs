// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel
{
    /// <summary>
    /// Implementation represents a <see cref="IEngineService"/> that has a persistent 
    /// state and is able to save/load it using <typeparamref name="TState"/>.
    /// </summary>
    public interface IStatefulService<TState> : IEngineService where TState : StateMap
    {
        Task SaveServiceStateAsync (TState state);
        Task LoadServiceStateAsync (TState state);
    }
}
