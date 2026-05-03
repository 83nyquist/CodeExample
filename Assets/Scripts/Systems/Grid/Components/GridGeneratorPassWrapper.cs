using Systems.Grid.AlterationPasses;
using UnityEngine;

namespace Systems.Grid.Components
{
    [System.Serializable]
    public class GridGeneratorPassWrapper
    {
        [SerializeReference]
        public IGridAlterationPass pass;
    }
}
