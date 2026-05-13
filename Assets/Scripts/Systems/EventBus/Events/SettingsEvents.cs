using Character;

namespace Systems.EventBus.Events
{
    public class VolumeChangedRequest : GameEvent
    {
        public int Value;
        public VolumeChangedRequest(int v) => Value = v;
    }

    public class GridRadiusChangedRequest : GameEvent
    {
        public int Value;
        public GridRadiusChangedRequest(int v) => Value = v;
    }

    public class PopulationSizeChangedRequest : GameEvent
    {
        public int Value;
        public PopulationSizeChangedRequest(int v) => Value = v;
    }

    public class VisionRadiusChangedRequest : GameEvent
    {
        public int Value;
        public VisionRadiusChangedRequest(int v) => Value = v;
    }

    public class FpsToggleRequest : GameEvent
    {
        public bool Value;
        public FpsToggleRequest(bool v) => Value = v;
    }

    public class CharacterAnimationEventsChangedEvent : GameEvent
    {
        public CharacterAnimationEvents Events;
        public CharacterAnimationEventsChangedEvent(CharacterAnimationEvents e) => Events = e;
    }
}
