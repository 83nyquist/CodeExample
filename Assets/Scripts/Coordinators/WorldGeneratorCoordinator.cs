using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Input;
using NPC;
using Systems.Decoration;
using Systems.EventBus;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Passes.Abstraction;
using UnityEngine;
using UserInterface;
using Zenject;

namespace Coordinators
{
    public class WorldGeneratorCoordinator : EventBusSubscriber
    {
        [Inject] private AxialHexGrid _grid;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private InputHandler _inputHandler;
        [Inject] private UiManager _uiManager;
        [Inject] private GenerationProgressTracker _progressTracker;
        [Inject] private NpcManager _npcManager;
        [Inject] private WorldDecorator _worldDecorator;

        [Header("Async Settings")]
        [SerializeField] private float maxMsPerFrame = 5f;
        [SerializeField] private bool generateOnAwake = true;

        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;

        [SerializeField] private List<GenerationPassWrapper> generationPasses = new();
        [SerializeField] private List<AlterationPassWrapper> alterationPasses = new();

        private GridGenerator _internalGenerator;
        private InputLock _inputLock;
        
        private int _currentSeed;
        private int _tilesInGrid;
        private bool _npcsComplete;
        private bool _visualsComplete;

        private Coroutine _generationRoutine;
        
        private void Start()
        {
            _internalGenerator = new GridGenerator(maxMsPerFrame);
            _inputLock = _inputHandler.RegisterInputLock(this);

            Subscribe<NpcSimulationCompleteEvent>(HandleNpcComplete);
            Subscribe<WorldVisualsReadyEvent>(HandleVisualsReady);
            Subscribe<GameStateChangedEvent>(HandleGameStateChangedEvent);
            
            if (generateOnAwake)
            {
                GenerateWorld();
            }
        }

        public void GenerateWorld()
        {
            Cleanup();
            Publish(new WorldGenerationStartedEvent());
            _generationRoutine = StartCoroutine(WorldGenerationFlow());
        }

        private void Cleanup()
        {
            if (_generationRoutine != null)
            {
                StopCoroutine(_generationRoutine);
                _generationRoutine = null;
            }
            
            Publish(new WorldCleanupEvent());
            
            _grid.ClearGrid();

            _npcsComplete = false;
            _visualsComplete = false;
            _currentSeed = 0;
        }

        private IEnumerator WorldGenerationFlow()
        {
            int radius = _playerSettings.gridRadius;
            _tilesInGrid = CalculateTotalTiles(radius);
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;
            
            // Calculate total work upfront
            int totalTileWorkEstimate = 0;
            totalTileWorkEstimate += _tilesInGrid * 2; // CreateDataRoutine + BuildNeighborsRoutine
            
            // Use the exact same pass list as the routines will use
            foreach (var pass in generationPasses)
                if (pass.pass != null) totalTileWorkEstimate += pass.pass.EstimateWorkUnits(_tilesInGrid);
                
            foreach (var pass in alterationPasses)
                if (pass.pass != null) totalTileWorkEstimate += pass.pass.EstimateWorkUnits(_tilesInGrid);
            
            // Ensure we capture the population size here to avoid issues if the slider is moved during generation
            int npcWorkLoad = _playerSettings.populationSize;
            
            Publish(new GenerationProgressInitializedEvent(totalTileWorkEstimate + _worldDecorator.GetInitialWorkEstimate(), npcWorkLoad));

            yield return _internalGenerator.CreateDataRoutine(_grid, radius, _tilesInGrid);
            yield return _internalGenerator.BuildNeighborsRoutine(_grid, radius, _tilesInGrid);
            
            // Publish(new GridStructuralDataReadyEvent(_grid.Hex.Tiles, _grid.Hex.HexSize));

            yield return RunGenerationPassesRoutine();
            yield return RunAlterationPassesRoutine();

            Publish(new GridInitializationFinishedEvent(_grid.Tiles, _grid.hexSize));

            // Wait for both NPCs and World Tiles to be visually and logically ready
            yield return new WaitUntil(() => _npcsComplete && _visualsComplete);
            
            Publish(new WorldGenerationFinishedEvent());
            Publish(new GameFlowInitUnlockRequest(ToString()));
            _generationRoutine = null;
        }

        private IEnumerator RunGenerationPassesRoutine() =>
            ProcessPassesRoutine(
                generationPasses.Select(w => w.pass).Where(p => p != null),
                pass => pass.Execute(_grid, _currentSeed),
                pass => pass.EstimateWorkUnits(_tilesInGrid)
            );

        private IEnumerator RunAlterationPassesRoutine() =>
            ProcessPassesRoutine(
                alterationPasses.Select(w => w.pass).Where(p => p != null),
                pass => pass.Execute(_grid, _currentSeed),
                pass => pass.EstimateWorkUnits(_tilesInGrid)
            );

        private IEnumerator ProcessPassesRoutine<T>(
            IEnumerable<T> passes,
            Action<T> executeAction,
            Func<T, int> estimateWork)
        {
            float budgetSeconds = maxMsPerFrame / 1000f;
            float lastYieldTime = Time.realtimeSinceStartup;
            int accumulatedWork = 0;

            foreach (var pass in passes)
            {
                executeAction(pass);

                int workDone = estimateWork(pass);
                accumulatedWork += workDone;

                // Since passes are few but heavy, we check budget after every pass.
                if (Time.realtimeSinceStartup - lastYieldTime > budgetSeconds)
                {
                    Publish(new ReportWorkProgressRequest(accumulatedWork, 0));
                    accumulatedWork = 0;
                    yield return null;
                    lastYieldTime = Time.realtimeSinceStartup;
                }
            }

            if (accumulatedWork > 0)
            {
                Publish(new ReportWorkProgressRequest(accumulatedWork, 0));
            }
        }

        private void HandleGameStateChangedEvent(GameStateChangedEvent obj)
        {
            if (obj.State == GameState.Initializing)
            {
                Publish(new GameFlowInitLockRequest(ToString()));
                GenerateWorld();
            }
        }

        private void HandleNpcComplete(NpcSimulationCompleteEvent e)
        {
            _npcsComplete = true;
        }
        
        private void HandleVisualsReady(WorldVisualsReadyEvent e)
        {
            _visualsComplete = true;
        }
        
        private int CalculateTotalTiles(int radius)
        {
            return 3 * radius * radius + 3 * radius + 1;
        }
        
        #region Editor Helpers
        public void AddGenerationPass(IGridGenerationPass pass)
        {
            generationPasses.Add(new GenerationPassWrapper { pass = pass });
        }
        
        public void RemoveGenerationPass(Type type)
        {
            generationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }
        
        public void AddAlterationPass(IGridAlterationPass pass)
        {
            alterationPasses.Add(new AlterationPassWrapper { pass = pass });
        }
        
        public void RemoveAlterationPass(Type type)
        {
            alterationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }
        
        public bool HasGenerationPass(Type type)
        {
            return generationPasses.Any(w => w.pass?.GetType() == type);
        }

        public bool HasAlterationPass(Type type)
        {
            return alterationPasses.Any(w => w.pass?.GetType() == type);
        }
        
        public void ClearGenerationPasses()
        {
            generationPasses.Clear();
        }

        public void ClearAlterationPasses()
        {
            alterationPasses.Clear();
        }
        #endregion
    }
}