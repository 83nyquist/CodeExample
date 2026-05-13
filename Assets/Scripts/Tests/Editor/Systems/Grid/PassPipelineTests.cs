using System;
using NUnit.Framework;
using Systems.Grid;
using Systems.Grid.Passes.Abstraction;

namespace Tests.Editor.Systems.Grid
{
    public class PassPipelineTests
    {
        private PassPipeline _pipeline;

        [SetUp]
        public void SetUp()
        {
            _pipeline = new PassPipeline();
        }

        [Test]
        public void NewPipeline_HasZeroPasses()
        {
            Assert.AreEqual(0, _pipeline.generationPasses.Count);
            Assert.AreEqual(0, _pipeline.alterationPasses.Count);
        }

        [Test]
        public void AddGenerationPass_IncreasesCount()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            Assert.AreEqual(1, _pipeline.generationPasses.Count);
        }

        [Test]
        public void AddAlterationPass_IncreasesCount()
        {
            _pipeline.AddAlterationPass(new TestAlterationPass());
            Assert.AreEqual(1, _pipeline.alterationPasses.Count);
        }

        [Test]
        public void HasGenerationPass_ReturnsTrue_AfterAdd()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            Assert.IsTrue(_pipeline.HasGenerationPass(typeof(TestGenerationPass)));
        }

        [Test]
        public void HasGenerationPass_ReturnsFalse_ForMissing()
        {
            Assert.IsFalse(_pipeline.HasGenerationPass(typeof(TestGenerationPass)));
        }

        [Test]
        public void RemoveGenerationPass_RemovesIt()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            _pipeline.RemoveGenerationPass(typeof(TestGenerationPass));
            Assert.IsFalse(_pipeline.HasGenerationPass(typeof(TestGenerationPass)));
            Assert.AreEqual(0, _pipeline.generationPasses.Count);
        }

        [Test]
        public void RemoveGenerationPass_RemovesOnlyMatchingType()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            _pipeline.AddGenerationPass(new SecondTestGenerationPass());
            _pipeline.RemoveGenerationPass(typeof(TestGenerationPass));
            Assert.IsFalse(_pipeline.HasGenerationPass(typeof(TestGenerationPass)));
            Assert.IsTrue(_pipeline.HasGenerationPass(typeof(SecondTestGenerationPass)));
        }

        [Test]
        public void RemoveGenerationPass_NonExistent_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _pipeline.RemoveGenerationPass(typeof(TestGenerationPass)));
        }

        [Test]
        public void ClearGenerationPasses_EmptiesList()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            _pipeline.AddGenerationPass(new SecondTestGenerationPass());
            _pipeline.ClearGenerationPasses();
            Assert.AreEqual(0, _pipeline.generationPasses.Count);
        }

        [Test]
        public void HasAlterationPass_ReturnsTrue_AfterAdd()
        {
            _pipeline.AddAlterationPass(new TestAlterationPass());
            Assert.IsTrue(_pipeline.HasAlterationPass(typeof(TestAlterationPass)));
        }

        [Test]
        public void HasAlterationPass_ReturnsFalse_ForMissing()
        {
            Assert.IsFalse(_pipeline.HasAlterationPass(typeof(TestAlterationPass)));
        }

        [Test]
        public void RemoveAlterationPass_RemovesIt()
        {
            _pipeline.AddAlterationPass(new TestAlterationPass());
            _pipeline.RemoveAlterationPass(typeof(TestAlterationPass));
            Assert.IsFalse(_pipeline.HasAlterationPass(typeof(TestAlterationPass)));
            Assert.AreEqual(0, _pipeline.alterationPasses.Count);
        }

        [Test]
        public void RemoveAlterationPass_NonExistent_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _pipeline.RemoveAlterationPass(typeof(TestAlterationPass)));
        }

        [Test]
        public void ClearAlterationPasses_EmptiesList()
        {
            _pipeline.AddAlterationPass(new TestAlterationPass());
            _pipeline.AddAlterationPass(new SecondTestAlterationPass());
            _pipeline.ClearAlterationPasses();
            Assert.AreEqual(0, _pipeline.alterationPasses.Count);
        }

        [Test]
        public void EstimateTotalWork_NoPasses_ReturnsZero()
        {
            Assert.AreEqual(0, _pipeline.EstimateTotalWork(100));
        }

        [Test]
        public void EstimateTotalWork_SumsAllPasses()
        {
            _pipeline.AddGenerationPass(new FixedWorkGenerationPass(50));
            _pipeline.AddAlterationPass(new FixedWorkAlterationPass(30));
            Assert.AreEqual(80, _pipeline.EstimateTotalWork(100));
        }

        [Test]
        public void EstimateTotalWork_MultipleOfEach_SumsCorrectly()
        {
            _pipeline.AddGenerationPass(new FixedWorkGenerationPass(10));
            _pipeline.AddGenerationPass(new FixedWorkGenerationPass(20));
            _pipeline.AddAlterationPass(new FixedWorkAlterationPass(30));
            _pipeline.AddAlterationPass(new FixedWorkAlterationPass(40));
            Assert.AreEqual(100, _pipeline.EstimateTotalWork(100));
        }

        [Test]
        public void EstimateTotalWork_PassesWithTotalTiles_ReceivesCorrectArgument()
        {
            _pipeline.AddGenerationPass(new TilesAwareGenerationPass());
            _pipeline.AddAlterationPass(new TilesAwareAlterationPass());
            Assert.AreEqual(450, _pipeline.EstimateTotalWork(150));
        }

        [Test]
        public void GenerationAndAlteration_AreIndependent()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            Assert.AreEqual(1, _pipeline.generationPasses.Count);
            Assert.AreEqual(0, _pipeline.alterationPasses.Count);

            _pipeline.AddAlterationPass(new TestAlterationPass());
            Assert.AreEqual(1, _pipeline.generationPasses.Count);
            Assert.AreEqual(1, _pipeline.alterationPasses.Count);
        }

        [Test]
        public void EstimateTotalWork_SkipsNullPassInWrapper()
        {
            _pipeline.generationPasses.Add(new GenerationPassWrapper { pass = null });
            _pipeline.alterationPasses.Add(new AlterationPassWrapper { pass = null });
            Assert.AreEqual(0, _pipeline.EstimateTotalWork(100));
        }

        [Test]
        public void AddDuplicateType_Allowed()
        {
            _pipeline.AddGenerationPass(new TestGenerationPass());
            _pipeline.AddGenerationPass(new TestGenerationPass());
            Assert.AreEqual(2, _pipeline.generationPasses.Count);
        }

        #region Mock pass implementations

        private class TestGenerationPass : BaseGenerationPass
        {
            public override string PassName => "TestGen";
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class SecondTestGenerationPass : BaseGenerationPass
        {
            public override string PassName => "SecondTestGen";
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class TestAlterationPass : BaseAlterationPass
        {
            public override string PassName => "TestAlt";
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class SecondTestAlterationPass : BaseAlterationPass
        {
            public override string PassName => "SecondTestAlt";
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class FixedWorkGenerationPass : BaseGenerationPass
        {
            private readonly int _workUnits;
            public override string PassName => "FixedGen";
            public FixedWorkGenerationPass(int workUnits) { _workUnits = workUnits; }
            public override int EstimateWorkUnits(int totalTiles) => _workUnits;
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class FixedWorkAlterationPass : BaseAlterationPass
        {
            private readonly int _workUnits;
            public override string PassName => "FixedAlt";
            public FixedWorkAlterationPass(int workUnits) { _workUnits = workUnits; }
            public override int EstimateWorkUnits(int totalTiles) => _workUnits;
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class TilesAwareGenerationPass : BaseGenerationPass
        {
            public override string PassName => "TilesAwareGen";
            public override int EstimateWorkUnits(int totalTiles) => totalTiles;
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        private class TilesAwareAlterationPass : BaseAlterationPass
        {
            public override string PassName => "TilesAwareAlt";
            public override int EstimateWorkUnits(int totalTiles) => totalTiles * 2;
            public override void Execute(AxialHexGrid grid, int seed) { }
        }

        #endregion
    }
}
