using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Input;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.Grid.Passes.Alteration;
using Systems.Grid.Passes.Generation;
using UnityEngine;
using UserInterface;
using Zenject;

namespace Systems.Coordinators
{
    [Serializable]
    public class GenerationPassWrapper
    {
        [SerializeReference] public IGridGenerationPass pass;
    }

    [Serializable]
    public class AlterationPassWrapper
    {
        [SerializeReference] public IGridAlterationPass pass;
    }

    public class WorldGeneratorCoordinator : MonoBehaviour
    {
        [Inject] private AxialHexGrid _grid;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private InputHandler _inputHandler;
        [Inject] private UiManager _uiManager;
        [Inject] private GenerationProgressTracker _progressTracker;

        [Header("Async Settings")]
        [SerializeField] private float maxMsPerFrame = 5f;
        [SerializeField] private bool generateOnAwake = true;

        [Header("Seed Settings")]
        [SerializeField] private bool useRandomSeed = true;
        [SerializeField] private int customSeed = 42;

        [SerializeField] private List<GenerationPassWrapper> generationPasses = new();
        [SerializeField] private List<AlterationPassWrapper> alterationPasses = new();

        public event Action OnGenerationStarted;
        public event Action OnGenerationComplete;

        private GridGenerator _internalGenerator;
        private InputLock _inputLock;
        private int _currentSeed;

        private void Start()
        {
            _internalGenerator = new GridGenerator(_progressTracker, maxMsPerFrame);
            _inputLock = _inputHandler.RegisterInputLock(this);

            if (generateOnAwake)
            {
                GenerateWorld();
            }
        }

        public void GenerateWorld()
        {
            StartCoroutine(WorldGenerationFlow());
        }

        private IEnumerator WorldGenerationFlow()
        {
            OnGenerationStarted?.Invoke();
            _inputLock.IsLocked = true;
            _uiManager.ShowLoadingScreen();

            int radius = _playerSettings.gridRadius;
            _progressTracker.Initialize(radius, _playerSettings.populationSize);
            _currentSeed = useRandomSeed ? UnityEngine.Random.Range(1, 999999) : customSeed;

            // 1. Structural Generation
            _grid.ClearGrid();
            yield return StartCoroutine(_internalGenerator.CreateDataRoutine(_grid, radius));
            yield return StartCoroutine(_internalGenerator.BuildNeighborsRoutine(_grid, radius));

            // 2. Generation Passes (Set TileType, Elevation, etc.)
            RunGenerationPasses();

            // 3. Alteration Passes (Rotation, Smoothing, Variation Indices)
            RunAlterationPasses();

            // 4. Finalization
            _inputLock.IsLocked = false;
            
            Debug.Log($"World Generation Complete. Seed: {_currentSeed}");
            OnGenerationComplete?.Invoke();
        }

        private void RunGenerationPasses()
        {
            var ordered = generationPasses
                .Select(w => w.pass)
                .Where(p => p != null);

            foreach (var pass in ordered) pass.Execute(_grid, _currentSeed);
        }

        private void RunAlterationPasses()
        {
            var ordered = alterationPasses
                .Select(w => w.pass)
                .Where(p => p != null);

            foreach (var pass in ordered) pass.Execute(_grid, _currentSeed);
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
        #endregion
    }
}