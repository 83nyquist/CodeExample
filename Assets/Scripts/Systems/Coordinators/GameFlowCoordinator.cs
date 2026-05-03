using System;
using Character;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Systems.Coordinators
{
    public enum GameState
    {
        Initializing,
        CharacterSelection,
        Playing
    }

    public class GameFlowCoordinator : MonoBehaviour
    {
        [Inject] private VanguardController _vanguardController;
        [Inject] private WorldGeneratorCoordinator _worldGenerator;

        public event Action<GameState> OnStateChanged;
        
        private GameState _currentState;
        private bool _isWorldReady;
        private bool _isCharacterSelected;

        private void Start()
        {
            _worldGenerator.OnGenerationComplete += HandleWorldReady;
            SetState(GameState.Initializing);
        }

        public void ResetWorldState()
        {
            _isWorldReady = false;
            _vanguardController.DeSpawn();
            // We don't necessarily reset _isCharacterSelected here 
            // in case the user wants to keep their leader choice across re-gens
            SetState(GameState.Initializing);
        }

        private void OnDestroy()
        {
            if (_worldGenerator != null) _worldGenerator.OnGenerationComplete -= HandleWorldReady;
        }

        public void SelectCharacter(CharacterItem leader)
        {
            _vanguardController.SetLeader(leader);
            _isCharacterSelected = true;
            CheckTransitionToGameplay();
        }

        private void HandleWorldReady()
        {
            _isWorldReady = true;
            
            if (!_isCharacterSelected)
            {
                SetState(GameState.CharacterSelection);
            }
            
            CheckTransitionToGameplay();
        }

        private void CheckTransitionToGameplay()
        {
            if (_isWorldReady && _isCharacterSelected)
            {
                _isWorldReady = false; 
                _isCharacterSelected = false;
                _vanguardController.Spawn();
                SetState(GameState.Playing);
            }
        }

        private void SetState(GameState newState)
        {
            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
        }
    }
}
