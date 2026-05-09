using System.Collections.Generic;
using Data;
using Systems.EventBus;
using UnityEngine;
using Zenject;

namespace Coordinators
{
    public enum GameState
    {
        Initializing,
        Playing
    }

    public class GameFlowCoordinator : EventBusSubscriber
    {
        [Inject] private SettingsSyncHandler _settingsSyncHandler;
        [Inject] private PlayerSettings _playerSettings;
        
        [Header("Debugging")]
        [SerializeField] private GameState currentState;

        private readonly HashSet<string> _activeInitBlockers = new();

        private void Start()
        {
            Subscribe<GameFlowInitLockRequest>(HandleInitLockRequest);
            Subscribe<GameFlowInitUnlockRequest>(HandleInitUnlockRequest);
            Subscribe<GenerateWorldRequest>(HandleGenerateWorldRequest);
            
            _settingsSyncHandler.Initialize();
            
            SetState(GameState.Initializing);
        }

        private void HandleGenerateWorldRequest(GenerateWorldRequest obj)
        {
            SetState(GameState.Initializing);
        }

        protected override void OnDestroy()
        {
            // Release any input locks held by this coordinator (custom cleanup)
            Publish(new InputLockRequest(this, false));
            
            // Base class handles unsubscription automatically
            base.OnDestroy();
        }

        private void HandleInitLockRequest(GameFlowInitLockRequest e)
        {
            _activeInitBlockers.Add(e.BlockerId);
            EvaluateState();
        }

        private void HandleInitUnlockRequest(GameFlowInitUnlockRequest e)
        {
            _activeInitBlockers.Remove(e.BlockerId);
            EvaluateState();
        }

        private void EvaluateState()
        {
            // Determine the current state based on high-priority blockers
            if (_activeInitBlockers.Count <= 0)
            {
                SetState(GameState.Playing);
            }
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            // Lock input in any state other than Playing
            Publish(new InputLockRequest(this, newState != GameState.Playing));
            Publish(new GameStateChangedEvent(newState));
        }
    }
}