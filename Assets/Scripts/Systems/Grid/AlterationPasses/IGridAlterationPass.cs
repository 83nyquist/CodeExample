namespace Systems.Grid.AlterationPasses
{
    public interface IGridAlterationPass
    {
        string PassName { get; }
        int Priority { get; } // Lower = runs first
        
        void Execute(AxialHexGrid grid, int seed);
    }
}