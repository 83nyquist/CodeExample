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
        Loading,
        Playing
    }

    public class GameFlowCoordinator : EventBusSubscriber
    {
        [Inject] private SettingsSyncHandler _settingsSyncHandler;
        [Inject] private PlayerSettings _playerSettings;
        
        [Header("Debugging")]
        [SerializeField] private GameState currentState = GameState.Initializing;

        [SerializeField] private List<string> activeInitBlockers;

        private void Start()
        {
            currentState = GameState.Initializing;
            
            Subscribe<GameFlowInitLockRequest>(HandleInitLockRequest);
            Subscribe<GameFlowInitUnlockRequest>(HandleInitUnlockRequest);
            Subscribe<GenerateWorldRequest>(HandleGenerateWorldRequest);
            
            _settingsSyncHandler.Initialize();
        }

        public void Initialize()
        {
            SetState(GameState.Loading);
        }

        private void HandleGenerateWorldRequest(GenerateWorldRequest obj)
        {
            SetState(GameState.Loading);
        }

        private void HandleInitLockRequest(GameFlowInitLockRequest e)
        {
            activeInitBlockers.Add(e.BlockerId);
            EvaluateState();
        }

        private void HandleInitUnlockRequest(GameFlowInitUnlockRequest e)
        {
            activeInitBlockers.Remove(e.BlockerId);
            EvaluateState();
        }

        private void EvaluateState()
        {
            // Determine the current state based on high-priority blockers
            if (activeInitBlockers.Count <= 0)
            {
                SetState(GameState.Playing);
            }
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            Publish(new GameStateChangedEvent(newState));
        }
    }
}