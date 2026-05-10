namespace Systems.Grid.Passes.Abstraction
{
    /// <summary>
    /// Defines the contract for a grid alteration pass that modifies existing tile data.
    /// </summary>
    public interface IGridAlterationPass
    {
        /// <summary>
        /// Gets the display name of the alteration pass.
        /// </summary>
        string PassName { get; }

        /// <summary>
        /// Estimates the amount of work units this pass will perform for progress tracking.
        /// </summary>
        /// <param name="totalTiles">The total number of tiles in the grid.</param>
        int EstimateWorkUnits(int totalTiles);

        /// <summary>
        /// Executes the alteration logic on the provided grid.
        /// </summary>
        /// <param name="grid">The hex grid to modify.</param>
        /// <param name="seed">The random seed for deterministic generation.</param>
        void Execute(AxialHexGrid grid, int seed);
    }
}
