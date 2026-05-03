using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Input;
using Systems.Grid;
using Systems.Grid.AlterationPasses;
using Systems.Grid.Components;
using UnityEngine;
using UserInterface;
using Zenject;

namespace Systems.Coordinators
{
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

        [SerializeField] private List<GridGeneratorPassWrapper> augmentationPasses = new();

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

            // 2. Logical Passes (Perlin, Biomes, etc.)
            RunAugmentationPasses();

            // 3. Finalization
            _inputLock.IsLocked = false;
            
            Debug.Log($"World Generation Complete. Seed: {_currentSeed}");
            OnGenerationComplete?.Invoke();
        }

        private void RunAugmentationPasses()
        {
            var ordered = augmentationPasses
                .Where(w => w.pass != null)
                .Select(w => w.pass)
                .OrderBy(p => p.Priority);

            foreach (var pass in ordered)
            {
                pass.Execute(_grid, _currentSeed);
            }
        }

        #region Editor Helpers
        public void AddGeneratorPass(IGridAlterationPass pass)
        {
            augmentationPasses.Add(new GridGeneratorPassWrapper { pass = pass });
        }

        public void RemoveGeneratorPass(IGridAlterationPass pass)
        {
            augmentationPasses.RemoveAll(w => w.pass == pass);
        }

        public List<IGridAlterationPass> GetGeneratorPasses()
        {
            return augmentationPasses
                .Where(w => w.pass != null)
                .Select(w => w.pass)
                .ToList();
        }
        #endregion
    }
}