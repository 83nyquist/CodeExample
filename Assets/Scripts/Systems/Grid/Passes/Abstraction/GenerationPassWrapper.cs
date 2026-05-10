using System;
using UnityEngine;

namespace Systems.Grid.Passes.Abstraction
{
    /// <summary>
    /// A serializable wrapper for polymorphic generation passes using SerializeReference.
    /// </summary>
    [Serializable]
    public class GenerationPassWrapper
    {
        /// <summary>
        /// The specific generation pass implementation.
        /// </summary>
        [SerializeReference] public IGridGenerationPass pass;
    }
}