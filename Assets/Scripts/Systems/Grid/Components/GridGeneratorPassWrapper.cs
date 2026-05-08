using Systems.Grid.Passes.Alteration;
using UnityEngine;
using IGridAlterationPass = Systems.Grid.Passes.Abstraction.IGridAlterationPass;

namespace Systems.Grid.Components
{
    [System.Serializable]
    public class GridGeneratorPassWrapper
    {
        [SerializeReference]
        public IGridAlterationPass pass;
    }
}
