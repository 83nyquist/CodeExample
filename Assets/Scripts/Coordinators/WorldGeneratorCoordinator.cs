using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Systems.Decoration;
using Systems.EventBus;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Passes.Abstraction;
using Systems.NonPlayerCharacters;
using Systems.NonPlayerCharacters.Components;
using UnityEngine;
using UserInterface;
using Zenject;

namespace Coordinators
{
    public class WorldGeneratorCoordinator : EventBusSubscriber
    {
        [Inject] private AxialHexGrid _grid;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private UiManager _uiManager;
        [Inject] private NpcManager _npcManager;
        [Inject] private WorldDecorator _worldDecorator;

        [Header("Generation Progress Tracker")]
        [SerializeField] private GenerationProgressTracker progressTracker;
        
        [Header("Async Settings")]
        [SerializeField] private float maxMsPerFrame = 5f;
        [SerializeField] private bool generateOnAwake = true;

        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;

        [SerializeField] private List<GenerationPassWrapper> generationPasses = new();
        [SerializeField] private List<AlterationPassWrapper> alterationPasses = new();

        private GridGenerator _internalGenerator;
        
        private int _currentSeed;
        private int _tilesInGrid;
        private bool _npcsComplete;
        private bool _visualsComplete;

        private Coroutine _generationRoutine;
        
        /// <summary>
        /// Initializes the generation system, progress tracker, and event subscriptions.
        /// </summary>
        private void Start()
        {
            progressTracker = new GenerationProgressTracker();
            _internalGenerator = new GridGenerator(maxMsPerFrame);

            Subscribe<NpcSimulationCompleteEvent>(HandleNpcComplete);
            Subscribe<WorldVisualsReadyEvent>(HandleVisualsReady);
            Subscribe<GameStateChangedEvent>(HandleGameStateChangedEvent);
            
            if (generateOnAwake)
            {
                GenerateWorld();
            }
        }

        /// <summary>
        /// Initiates the world generation process.
        /// </summary>
        public void GenerateWorld()
        {
            Cleanup();
            Publish(new WorldGenerationStartedEvent());
            _generationRoutine = StartCoroutine(WorldGenerationFlow());
        }

        /// <summary>
        /// Resets the generation state and stops any active generation routines.
        /// </summary>
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

        /// <summary>
        /// Orchestrates the sequence of world generation steps.
        /// </summary>
        private IEnumerator WorldGenerationFlow()
        {
            int radius = _playerSettings.GridRadius;
            _tilesInGrid = CalculateTotalTiles(radius);
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;
            
            int totalTileWorkEstimate = 0;
            totalTileWorkEstimate += _tilesInGrid * 2;
            
            foreach (var pass in generationPasses)
                if (pass.pass != null) totalTileWorkEstimate += pass.pass.EstimateWorkUnits(_tilesInGrid);
                
            foreach (var pass in alterationPasses)
                if (pass.pass != null) totalTileWorkEstimate += pass.pass.EstimateWorkUnits(_tilesInGrid);
            
            int npcWorkLoad = _playerSettings.PopulationSize;
            
            Publish(new GenerationProgressInitializedEvent(totalTileWorkEstimate + _worldDecorator.GetInitialWorkEstimate(), npcWorkLoad));

            yield return _internalGenerator.CreateDataRoutine(_grid, radius, _tilesInGrid);
            yield return _internalGenerator.BuildNeighborsRoutine(_grid, radius, _tilesInGrid);

            yield return RunGenerationPassesRoutine();
            yield return RunAlterationPassesRoutine();

            Publish(new GridInitializationFinishedEvent(_grid.Tiles, _grid.hexSize));

            yield return new WaitUntil(() => _npcsComplete && _visualsComplete);
            
            Publish(new WorldGenerationFinishedEvent());
            Publish(new GameFlowInitUnlockRequest(ToString()));
            _generationRoutine = null;
        }

        /// <summary>
        /// Executes the generation passes.
        /// </summary>
        private IEnumerator RunGenerationPassesRoutine() =>
            ProcessPassesRoutine(
                generationPasses.Select(w => w.pass).Where(p => p != null),
                pass => pass.Execute(_grid, _currentSeed),
                pass => pass.EstimateWorkUnits(_tilesInGrid)
            );

        /// <summary>
        /// Executes the alteration passes.
        /// </summary>
        private IEnumerator RunAlterationPassesRoutine() =>
            ProcessPassesRoutine(
                alterationPasses.Select(w => w.pass).Where(p => p != null),
                pass => pass.Execute(_grid, _currentSeed),
                pass => pass.EstimateWorkUnits(_tilesInGrid)
            );

        /// <summary>
        /// Generic routine to process a collection of passes within a time budget.
        /// </summary>
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

        /// <summary>
        /// Handles changes to the game state.
        /// </summary>
        private void HandleGameStateChangedEvent(GameStateChangedEvent obj)
        {
            if (obj.State == GameState.Loading)
            {
                Publish(new GameFlowInitLockRequest(ToString()));
                GenerateWorld();
            }
        }

        /// <summary>
        /// Handles the completion of NPC simulation.
        /// </summary>
        private void HandleNpcComplete(NpcSimulationCompleteEvent e)
        {
            _npcsComplete = true;
        }
        
        /// <summary>
        /// Handles the completion of world visual decoration.
        /// </summary>
        private void HandleVisualsReady(WorldVisualsReadyEvent e)
        {
            _visualsComplete = true;
        }
        
        /// <summary>
        /// Calculates total tiles for a hex grid of a given radius.
        /// </summary>
        private int CalculateTotalTiles(int radius)
        {
            return 3 * radius * radius + 3 * radius + 1;
        }
        
        /// <summary>
        /// Adds a generation pass to the pipeline.
        /// </summary>
        public void AddGenerationPass(IGridGenerationPass pass)
        {
            generationPasses.Add(new GenerationPassWrapper { pass = pass });
        }
        
        /// <summary>
        /// Removes a generation pass of a specific type.
        /// </summary>
        public void RemoveGenerationPass(Type type)
        {
            generationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }
        
        /// <summary>
        /// Adds an alteration pass to the pipeline.
        /// </summary>
        public void AddAlterationPass(IGridAlterationPass pass)
        {
            alterationPasses.Add(new AlterationPassWrapper { pass = pass });
        }
        
        /// <summary>
        /// Removes an alteration pass of a specific type.
        /// </summary>
        public void RemoveAlterationPass(Type type)
        {
            alterationPasses.RemoveAll(w => w.pass?.GetType() == type);
        }
        
        /// <summary>
        /// Checks if a specific generation pass type exists.
        /// </summary>
        public bool HasGenerationPass(Type type)
        {
            return generationPasses.Any(w => w.pass?.GetType() == type);
        }

        /// <summary>
        /// Checks if a specific alteration pass type exists.
        /// </summary>
        public bool HasAlterationPass(Type type)
        {
            return alterationPasses.Any(w => w.pass?.GetType() == type);
        }
        
        /// <summary>
        /// Clears all generation passes.
        /// </summary>
        public void ClearGenerationPasses()
        {
            generationPasses.Clear();
        }

        /// <summary>
        /// Clears all alteration passes.
        /// </summary>
        public void ClearAlterationPasses()
        {
            alterationPasses.Clear();
        }
    }
}