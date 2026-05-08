using System;
using UnityEngine;

namespace Systems.Grid.Passes.Abstraction
{
    [Serializable]
    public class GenerationPassWrapper
    {
        [SerializeReference] public IGridGenerationPass pass;
    }
}