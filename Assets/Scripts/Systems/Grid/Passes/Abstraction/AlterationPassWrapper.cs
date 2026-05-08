using System;
using UnityEngine;

namespace Systems.Grid.Passes.Abstraction
{
    [Serializable]
    public class AlterationPassWrapper
    {
        [SerializeReference] public IGridAlterationPass pass;
    }
}
