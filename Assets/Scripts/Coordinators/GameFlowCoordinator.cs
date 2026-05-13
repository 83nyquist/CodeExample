using System.Collections.Generic;
using Systems.EventBus.BaseClasses;
using Systems.EventBus.Events;
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

    [DefaultExecutionOrder(-100)]
    public class GameFlowCoordinator : EventBusSubscriber
    {
        [Inject] private SettingsSyncHandler _settingsSyncHandler;
        
        [Header("Debugging")]
        [SerializeField] private GameState currentState = GameState.Initializing;

        [SerializeField] private List<string> activeInitBlockers;

        /// <summary>
        /// Initializes the coordinator by setting the initial state, subscribing to events, and initializing the settings sync handler.
        /// </summary>
        private void Start()
        {
            currentState = GameState.Initializing;
            
            Subscribe<GameFlowInitLockRequest>(HandleInitLockRequest);
            Subscribe<GameFlowInitUnlockRequest>(HandleInitUnlockRequest);
            Subscribe<GenerateWorldRequest>(HandleGenerateWorldRequest);
            
            _settingsSyncHandler.Initialize();
        }

        /// <summary>
        /// Triggers the transition to the loading state.
        /// </summary>
        public void Initialize()
        {
            SetState(GameState.Loading);
        }

        /// <summary>
        /// Handles the request to generate the world by transitioning the game state to loading.
        /// </summary>
        private void HandleGenerateWorldRequest(GenerateWorldRequest obj)
        {
            SetState(GameState.Loading);
        }

        /// <summary>
        /// Adds a blocker to the initialization queue and re-evaluates the game state.
        /// </summary>
        private void HandleInitLockRequest(GameFlowInitLockRequest e)
        {
            activeInitBlockers.Add(e.BlockerId);
            EvaluateState();
        }

        /// <summary>
        /// Removes a blocker from the initialization queue and re-evaluates the game state.
        /// </summary>
        private void HandleInitUnlockRequest(GameFlowInitUnlockRequest e)
        {
            activeInitBlockers.Remove(e.BlockerId);
            EvaluateState();
        }

        /// <summary>
        /// Evaluates if the game can transition to the playing state based on active blockers.
        /// </summary>
        private void EvaluateState()
        {
            if (activeInitBlockers.Count <= 0)
            {
                SetState(GameState.Playing);
            }
        }

        /// <summary>
        /// Updates the current game state and publishes a change event to the bus.
        /// </summary>
        private void SetState(GameState newState)
        {
            currentState = newState;
            Publish(new GameStateChangedEvent(newState));
        }
    }
}