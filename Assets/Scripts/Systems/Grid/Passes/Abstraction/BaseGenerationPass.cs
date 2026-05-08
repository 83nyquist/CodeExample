namespace Systems.Grid.Passes.Abstraction
{
    [System.Serializable]
    public abstract class BaseGenerationPass : IGridGenerationPass
    {
        public bool debugLog = false;
    
        public abstract string PassName { get; }
    
        public virtual int EstimateWorkUnits(int totalTiles) => totalTiles;

        public abstract void Execute(AxialHexGrid grid, int seed);
    }
}
