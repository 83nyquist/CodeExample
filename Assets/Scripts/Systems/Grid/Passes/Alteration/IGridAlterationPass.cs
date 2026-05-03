namespace Systems.Grid.Passes.Alteration
{
    public interface IGridAlterationPass
    {
        string PassName { get; }

        void Execute(AxialHexGrid grid, int seed);
    }
}