using System;
using UnityEngine;

namespace Systems.Grid.Passes.Abstraction
{
    /// <summary>
    /// A serializable wrapper for polymorphic alteration passes using SerializeReference.
    /// </summary>
    [Serializable]
    public class AlterationPassWrapper
    {
        /// <summary>
        /// The specific alteration pass implementation.
        /// </summary>
        [SerializeReference] public IGridAlterationPass pass;
    }
}
