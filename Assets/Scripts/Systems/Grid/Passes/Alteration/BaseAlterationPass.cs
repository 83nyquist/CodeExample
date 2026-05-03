namespace Systems.Grid.Passes.Alteration
{
    [System.Serializable]
    public abstract class BaseAlterationPass : IGridAlterationPass
    {
        public bool debugLog = false;
    
        public abstract string PassName { get; }
    
        public abstract void Execute(AxialHexGrid grid, int seed);
    }
}
