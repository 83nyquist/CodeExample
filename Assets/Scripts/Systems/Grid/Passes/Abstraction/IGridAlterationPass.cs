namespace Systems.Grid.Passes.Abstraction
{
    public interface IGridAlterationPass
    {
        string PassName { get; }

        int EstimateWorkUnits(int totalTiles);
        void Execute(AxialHexGrid grid, int seed);
    }
}
