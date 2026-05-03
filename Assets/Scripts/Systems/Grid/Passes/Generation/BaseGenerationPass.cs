namespace Systems.Grid.Passes.Generation
{
    [System.Serializable]
    public abstract class BaseGenerationPass : IGridGenerationPass
    {
        public bool debugLog = false;
    
        public abstract string PassName { get; }
    
        public abstract void Execute(AxialHexGrid grid, int seed);
    }
}