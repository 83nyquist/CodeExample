using UnityEngine;

namespace Systems.Grid.AlterationPasses
{
    [System.Serializable]
    public abstract class BaseAlterationPass : IGridAlterationPass
    {
        [Range(0, 100)]
        public int priority = 10;
    
        public abstract string PassName { get; }
        public int Priority 
        { 
            get => priority;
            set => priority = value;
        }
    
        public abstract void Execute(AxialHexGrid grid, int seed);
    }
}
