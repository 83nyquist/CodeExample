namespace Systems.Grid.Passes.Generation
{
    public interface IGridGenerationPass
    {
        string PassName { get; }
        
        void Execute(AxialHexGrid grid, int seed);
    }
}