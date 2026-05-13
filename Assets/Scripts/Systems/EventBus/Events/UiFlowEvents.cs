using Character;
using Coordinators;

namespace Systems.EventBus.Events
{
    public class GameStateChangedEvent : GameEvent
    {
        public GameState State { get; }
        public GameStateChangedEvent(GameState state) => State = state;
    }

    public class GenerateWorldRequest : GameEvent { }

    public class RespawnRequest : GameEvent { }

    public class ResetWorldRequest : GameEvent { }

    public class CommanderSelectedRequest : GameEvent
    {
        public CharacterItem Character { get; }
        public CommanderSelectedRequest(CharacterItem c) => Character = c;
    }

    public class ToggleNpcDebugRequest : GameEvent { }

    public class PlayerMoveRequest : GameEvent { }

    public class ClearPathRequest : GameEvent { }

    public class GameFlowInitLockRequest : GameEvent
    {
        public string BlockerId { get; }
        public GameFlowInitLockRequest(string blockerId) => BlockerId = blockerId;
    }

    public class GameFlowInitUnlockRequest : GameEvent
    {
        public string BlockerId { get; }
        public GameFlowInitUnlockRequest(string blockerId) => BlockerId = blockerId;
    }

    public class InputLockRequest : GameEvent
    {
        public string BlockerId { get; }
        public InputLockRequest(string blockerId) => BlockerId = blockerId;
    }

    public class InputUnlockRequest : GameEvent
    {
        public string BlockerId { get; }
        public InputUnlockRequest(string blockerId) => BlockerId = blockerId;
    }
}
