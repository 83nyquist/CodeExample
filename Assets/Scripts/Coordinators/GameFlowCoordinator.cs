using Systems.EventBus;
using Vanguard;
using Zenject;

namespace Coordinators
{
    public enum GameState
    {
        Initializing,
        CharacterSelection,
        Playing
    }

    public class GameFlowCoordinator : EventBusSubscriber
    {
        [Inject] private VanguardController _vanguardController;
        [Inject] private WorldGeneratorCoordinator _worldGenerator;
        
        private GameState _currentState;
        private bool _isWorldReady;
        private bool _isCharacterSelected;

        private void Start()
        {
            Subscribe<CommanderSelectedRequest>(SelectCharacter);
            Subscribe<WorldGenerationFinishedEvent>(HandleWorldReady);
            SetState(GameState.Initializing);
        }

        public void ResetWorldState()
        {
            _isWorldReady = false;
            _vanguardController.DeSpawn();
            
            SetState(GameState.Initializing);
        }

        public void SelectCharacter(CommanderSelectedRequest obj)
        {
            _vanguardController.SetLeader(obj.Character);
            _isCharacterSelected = true;
            CheckTransitionToGameplay();
        }

        private void HandleWorldReady(WorldGenerationFinishedEvent e)
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
            Publish(new GameStateChangedEvent(_currentState));
        }
    }
}
