namespace Systems.Grid.Passes.Abstraction
{
    /// <summary>
    /// Provides a base implementation for grid generation passes.
    /// </summary>
    [System.Serializable]
    public abstract class BaseGenerationPass : IGridGenerationPass
    {
        /// <summary>
        /// Determines if debug information should be logged during execution.
        /// </summary>
        public bool debugLog = false;
    
        /// <summary>
        /// Gets the display name of the generation pass.
        /// </summary>
        public abstract string PassName { get; }
    
        /// <summary>
        /// Default implementation for estimating work units based on total tile count.
        /// </summary>
        public virtual int EstimateWorkUnits(int totalTiles) => totalTiles;

        /// <summary>
        /// Executes the specific generation logic.
        /// </summary>
        /// <param name="grid">The hex grid to modify.</param>
        /// <param name="seed">The random seed for deterministic generation.</param>
        public abstract void Execute(AxialHexGrid grid, int seed);
    }
}
