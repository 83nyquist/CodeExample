using System;
using System.Collections.Generic;
using System.Linq;

namespace Systems.Grid.Passes.Abstraction
{
    [Serializable]
    public class PassPipeline
    {
        public List<GenerationPassWrapper> generationPasses = new();
        public List<AlterationPassWrapper> alterationPasses = new();

        public int EstimateTotalWork(int totalTiles)
        {
            int total = 0;
            foreach (var wrapper in generationPasses)
                if (wrapper.pass != null) total += wrapper.pass.EstimateWorkUnits(totalTiles);
            foreach (var wrapper in alterationPasses)
                if (wrapper.pass != null) total += wrapper.pass.EstimateWorkUnits(totalTiles);
            return total;
        }

        public void AddGenerationPass(IGridGenerationPass pass)
        {
            generationPasses.Add(new GenerationPassWrapper { pass = pass });
        }

        public void RemoveGenerationPass(Type type)
        {
            generationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }

        public bool HasGenerationPass(Type type)
        {
            return generationPasses.Any(w => w.pass?.GetType() == type);
        }

        public void ClearGenerationPasses()
        {
            generationPasses.Clear();
        }

        public void AddAlterationPass(IGridAlterationPass pass)
        {
            alterationPasses.Add(new AlterationPassWrapper { pass = pass });
        }

        public void RemoveAlterationPass(Type type)
        {
            alterationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }

        public bool HasAlterationPass(Type type)
        {
            return alterationPasses.Any(w => w.pass?.GetType() == type);
        }

        public void ClearAlterationPasses()
        {
            alterationPasses.Clear();
        }
    }
}
