using Systems.Grid.Passes.Alteration;
using UnityEngine;
using IGridAlterationPass = Systems.Grid.Passes.Abstraction.IGridAlterationPass;

namespace Systems.Grid.Components
{
    [System.Serializable]
    public class GridGeneratorPassWrapper
    {
        /// <summary>
        /// The specific grid alteration pass implementation to be executed.
        /// </summary>
        [SerializeReference]
        public IGridAlterationPass pass;
    }
}
