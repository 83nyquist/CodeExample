using System;
using System.Collections.Generic;
using System.IO;
using Systems.Decoration.Components;
using Systems.Grid.Components;
using UnityEngine;

// ============================================
// IMPORTANT NOTICE! This file combines all EventBus functionality into a single location 
// to facilitate an easier AI context workflow and architectural overview.
// ============================================

namespace Systems.EventBus
{
#region Infrastructure Enums
    /// <summary>
    /// Logging level for EventBusSubscriber debugging.
    /// </summary>
    public enum EventBusLogLevel
    {
        None,       // No logging
        Warning,    // Only warnings and errors
        Verbose     // All subscription/unsubscription activity
    }
#endregion

#region Base Event Class
    /// <summary>
    /// Base class for all events. Provides debugging information.
    /// When you hit a breakpoint, inspect these fields to see who published the event.
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>The class/file name that published this event</summary>
        public string Source { get; set; }
        
        /// <summary>The method name that published this event</summary>
        public string SourceMember { get; set; }
        
        /// <summary>When this event was published (UTC)</summary>
        public DateTime Timestamp { get; set; }
        
        protected GameEvent()
        {
            Source = "Unknown";
            SourceMember = "Unknown";
            Timestamp = DateTime.UtcNow;
        }
        
        public override string ToString()
        {
            return $"[{GetType().Name}] from {Source}.{SourceMember} at {Timestamp:T}";
        }
    }
#endregion

#region Grid System Events
    /// <summary>Published when all tile data has been purged from the repository.</summary>
    public class GridClearedEvent : GameEvent { }

    /// <summary>Published when basic tile coordinate data is allocated and ready for structural queries.</summary>
    // public class GridStructuralDataReadyEvent : GameEvent
    // {
    //     public IReadOnlyDictionary<Vector2Int, TileData> Tiles { get; }
    //     public float HexSize { get; }
    //
    //     public GridStructuralDataReadyEvent(IReadOnlyDictionary<Vector2Int, TileData> tiles, float hexSize) 
    //         => (Tiles, HexSize) = (tiles, hexSize);
    // }

    /// <summary>Published when all generation and alteration passes are finished and the grid is "final".</summary>
    public class GridInitializationFinishedEvent : GameEvent
    { 
        public IReadOnlyDictionary<Vector2Int, TileData> Tiles { get; }
        public int TotalTiles { get; }
        public float HexSize { get; }

        public GridInitializationFinishedEvent(IReadOnlyDictionary<Vector2Int, TileData> tiles, float hexSize) 
            => (Tiles, TotalTiles, HexSize) = (tiles, tiles.Count, hexSize);
    }

    /// <summary>Published when the set of tiles within the player's vision radius changes.</summary>
    public class VisibleTilesCountChangedEvent : GameEvent
    {
        public int Count { get; }
        public VisibleTilesCountChangedEvent(int count) => Count = count;
    }
#endregion

#region NPC System Events
    /// <summary>Published when the NPC manager has finished spawning and initializing its simulation.</summary>
    public class NpcSimulationCompleteEvent : GameEvent
    { 
        public int TotalAgents { get; }
        public NpcSimulationCompleteEvent(int totalAgents) => TotalAgents = totalAgents;
    }

    /// <summary>Request sent to update which NPCs are currently considered "visible" by the simulation.</summary>
    public class NpcVisibilityUpdateRequest : GameEvent
    {
        public HashSet<TileData> VisionSet { get; }
        public bool ForceVisible { get; }
        public NpcVisibilityUpdateRequest(HashSet<TileData> visionSet, bool force)
        {
            VisionSet = visionSet;
            ForceVisible = force;
        }
    }

    /// <summary>Published when the number of NPCs currently rendered on screen changes.</summary>
    public class NpcVisibleAgentsCountChangedEvent : GameEvent
    {
        public int VisibleCount { get; }
        public NpcVisibleAgentsCountChangedEvent(int visibleCount) => VisibleCount = visibleCount;
    }
#endregion

#region World Generation Events
    /// <summary>Published when the WorldGeneratorCoordinator begins the generation sequence.</summary>
    public class WorldGenerationStartedEvent : GameEvent { }

    /// <summary>Published when the entire world generation flow, including sub-systems, is fully complete.</summary>
    public class WorldGenerationFinishedEvent : GameEvent { }
    
    /// <summary>Published when the decorator has finished spawning the initial batch of world tiles.</summary>
    public class WorldVisualsReadyEvent : GameEvent { }

    /// <summary>Published to initialize the loading bar with a specific number of work units.</summary>
    public class GenerationProgressInitializedEvent : GameEvent
    {
        public int TotalTileWorkUnits { get; }
        public int TotalNpcWorkUnits { get; }
        public GenerationProgressInitializedEvent(int totalTileWorkUnits, int totalNpcWorkUnits)
        {
            TotalTileWorkUnits = totalTileWorkUnits;
            TotalNpcWorkUnits = totalNpcWorkUnits;
        }
    }
    
    /// <summary>Published when a specific generation pass completes to update the progress UI.</summary>
    public class GenerationProgressUpdatedEvent : GameEvent
    {
        public float Progress { get; }
        public float CompletedTileWorkUnits { get; }
        public float CompletedNpcWorkUnits { get; }
        public float TotalTileWorkUnits { get; }
        public float TotalNpcWorkUnits { get; }
        public GenerationProgressUpdatedEvent(float progress, float completedTileWorkUnits, float totalTileWorkUnits, float completedNpcWorkUnits, float totalNpcWorkUnits)
        {
            Progress = progress;
            CompletedNpcWorkUnits = completedNpcWorkUnits;
            TotalNpcWorkUnits = totalNpcWorkUnits;
            CompletedTileWorkUnits = completedTileWorkUnits;
            TotalTileWorkUnits = totalTileWorkUnits;
        }
    }

    /// <summary>Request to report a specific amount of completed work units.</summary>
    public class ReportWorkProgressRequest : GameEvent
    {
        public int AmountTiles { get; }
        public int AmountNpc { get; }
        public ReportWorkProgressRequest(int amountTiles, int amountNpc)
        {
            AmountTiles = amountTiles;
            AmountNpc = amountNpc;
        }
    }
    
    /// <summary>
    /// Published when the world is being reset or a generation is cancelled.
    /// Systems like NpcManager should listen for this to stop jobs and clear data.
    /// </summary>
    public class WorldCleanupEvent : GameEvent { }
#endregion

#region Player Movement Events
    /// <summary>Published when the player logically moves from one hex tile to another.</summary>
    public class PlayerMovedEvent : GameEvent
    {
        public TileData NewTile { get; }
        public PlayerMovedEvent(TileData tile) => NewTile = tile;
    }

    /// <summary>Published when the player reaches their targeted destination tile.</summary>
    public class PlayerDestinationReachedEvent : GameEvent
    {
        public TileData Tile { get; }
        public PlayerDestinationReachedEvent(TileData tile) => Tile = tile;
    }
#endregion

#region Path Drawing Events
    /// <summary>Request to calculate and draw a path to a specific target decorator.</summary>
    public class DrawPathRequest : GameEvent
    {
        public TileDecorator Target { get; }
        public DrawPathRequest(TileDecorator target) => Target = target;
    }

    /// <summary>Published when a valid path has been calculated for rendering.</summary>
    public class PathCreatedEvent : GameEvent
    {
        public List<TileData> Path { get; }
        public PathCreatedEvent(List<TileData> path) => Path = path;
    }

    /// <summary>Request to remove path visuals from the world.</summary>
    public class PathClearedEvent : GameEvent { }
#endregion

#region UI / Flow Requests & State
    /// <summary>Published when the global game state (Loading, Playing, etc.) changes.</summary>
    public class GameStateChangedEvent : GameEvent
    {
        public Coordinators.GameState State { get; }
        public GameStateChangedEvent(Coordinators.GameState state) => State = state;
    }

    /// <summary>User-driven request to generate a completely new world.</summary>
    public class GenerateWorldRequest : GameEvent { }

    /// <summary>Request to move the player back to the starting location.</summary>
    public class RespawnRequest : GameEvent { }

    /// <summary>Request to completely reset the world and UI to initial states.</summary>
    public class ResetWorldRequest : GameEvent { }
    
    /// <summary>Published when a user selects a character in the customization UI.</summary>
    public class CommanderSelectedRequest : GameEvent
    { 
        public Character.CharacterItem Character { get; } 
        public CommanderSelectedRequest(Character.CharacterItem c) => Character = c; 
    }
    
    /// <summary>Request to toggle debug visuals for NPCs.</summary>
    public class ToggleNpcDebugRequest : GameEvent { }

    /// <summary>Request to initiate player movement logic.</summary>
    public class PlayerMoveRequest : GameEvent { }

    /// <summary>Request to clear the current navigation path.</summary>
    public class ClearPathRequest : GameEvent { }
    
    /// <summary>Request to lock or unlock player input (e.g., during UI focus).</summary>
    public class InputLockRequest : GameEvent
    { 
        public bool IsLocked { get; set; }
    
        public InputLockRequest(object s, bool l) 
        { 
            Source = s?.ToString() ?? "Unknown";
            IsLocked = l; 
        } 
    }

    /// <summary>Request to add a blocker to the game flow, preventing state transitions to 'Playing'.</summary>
    public class GameFlowInitLockRequest : GameEvent
    {
        public string BlockerId { get; }
        public GameFlowInitLockRequest(string blockerId) => BlockerId = blockerId;
    }

    /// <summary>Request to remove a blocker from the game flow, potentially allowing state transitions.</summary>
    public class GameFlowInitUnlockRequest : GameEvent
    {
        public string BlockerId { get; }
        public GameFlowInitUnlockRequest(string blockerId) => BlockerId = blockerId;
    }
#endregion

#region Settings Requests
    /// <summary>Request to update the global audio volume.</summary>
    public class VolumeChangedRequest : GameEvent
    { 
        public int Value; 
        public VolumeChangedRequest(int v) => Value = v; 
    }
    
    /// <summary>Request to change the world generation grid radius.</summary>
    public class GridRadiusChangedRequest : GameEvent
    { 
        public int Value; 
        public GridRadiusChangedRequest(int v) => Value = v; 
    }
    
    /// <summary>Request to change the number of NPCs in the simulation.</summary>
    public class PopulationSizeChangedRequest : GameEvent
    { 
        public int Value; 
        public PopulationSizeChangedRequest(int v) => Value = v; 
    }
    
    /// <summary>Request to change the player's fog-of-war vision radius.</summary>
    public class VisionRadiusChangedRequest : GameEvent
    { 
        public int Value; 
        public VisionRadiusChangedRequest(int v) => Value = v; 
    }
    
    /// <summary>Request to toggle the FPS counter display.</summary>
    public class FpsToggleRequest : GameEvent
    { 
        public bool Value; 
        public FpsToggleRequest(bool v) => Value = v; 
    }

    /// <summary>Published when character animation metadata has been updated.</summary>
    public class CharacterAnimationEventsChangedEvent : GameEvent
    { 
        public Character.CharacterAnimationEvents Events; 
        public CharacterAnimationEventsChangedEvent(Character.CharacterAnimationEvents e) => Events = e; 
    }
#endregion

#region Mouse Input Events
    /// <summary>Published when the mouse scroll wheel is moved.</summary>
    public class MouseScrollEvent : GameEvent
    { 
        public float Delta; 
        public MouseScrollEvent(float delta) => Delta = delta; 
    }
    
    /// <summary>Published when a tile is clicked down.</summary>
    public class TilePointerDownEvent : GameEvent
    { 
        public TileDecorator Decorator { get; } 
        public TilePointerDownEvent(TileDecorator d) => Decorator = d; 
    }
    
    /// <summary>Published when the mouse button is released over a tile.</summary>
    public class TilePointerUpEvent : GameEvent
    { 
        public TileDecorator Decorator; 
        public TilePointerUpEvent(TileDecorator d) => Decorator = d; 
    }
    
    /// <summary>Published when the mouse is dragged across a tile.</summary>
    public class TileDragEvent : GameEvent
    { 
        public TileDecorator Decorator; 
        public TileDragEvent(TileDecorator d) => Decorator = d; 
    }
#endregion

#region Core Event Bus Engine
    /// <summary>
    /// The static engine that handles registration and dispatch of all game events.
    /// </summary>
    public static class EventBusSystem
    {
        private static readonly Dictionary<Type, List<Delegate>> EventSubscribers = new();

        /// <summary>Subscribes a listener to a specific event type.</summary>
        public static void Subscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (!EventSubscribers.ContainsKey(eventType))
            {
                EventSubscribers.Add(eventType, new List<Delegate>());
            }
            
            if (!EventSubscribers[eventType].Contains(listener))
            {
                EventSubscribers[eventType].Add(listener);
            }
        }

        /// <summary>Unsubscribes a listener from a specific event type.</summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> listener) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (EventSubscribers.ContainsKey(eventType))
            {
                EventSubscribers[eventType].Remove(listener);
                if (EventSubscribers[eventType].Count == 0)
                {
                    EventSubscribers.Remove(eventType);
                }
            }
        }

        /// <summary>Publishes an event to all subscribed listeners.</summary>
        public static void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            if (EventSubscribers.TryGetValue(eventType, out var listeners))
            {
                List<Delegate> listenersCopy = new List<Delegate>(listeners);
                foreach (Delegate listener in listenersCopy)
                {
                    (listener as Action<TEvent>)?.Invoke(eventToPublish);
                }
            }
        }
        
        /// <summary>Debug only: Get the current subscriber count for an event type.</summary>
        public static int GetSubscriberCount<TEvent>() where TEvent : class
        {
            Type eventType = typeof(TEvent);
            return EventSubscribers.TryGetValue(eventType, out var listeners) ? listeners.Count : 0;
        }
    }
    
#endregion

#region Event Bus Subscriber Base (MonoBehaviour)
    /// <summary>
    /// Base class for MonoBehaviours that need to subscribe to EventBusSystem events.
    /// Automatically handles subscribe/resubscribe on enable/disable/destroy.
    /// </summary>
    public abstract class EventBusSubscriber : MonoBehaviour
    {
        [Header("Event Bus Settings")]
        [SerializeField] protected EventBusLogLevel logLevel = EventBusLogLevel.Warning;
        
        private List<(Type type, Delegate handler)> _subscriptions = new();

        protected virtual EventBusLogLevel GetLogLevel() => logLevel;

        /// <summary>Subscribe to an event. Automatically tracks for cleanup.</summary>
        protected void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            Type eventType = typeof(TEvent);
            if (IsSubscribed(handler)) return;

            EventBusSystem.Subscribe(handler);
            _subscriptions.Add((eventType, handler));
        }

        /// <summary>Unsubscribe from a specific event.</summary>
        protected void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;
            
            Type eventType = typeof(TEvent);
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                if (_subscriptions[i].type == eventType && _subscriptions[i].handler == handler as Delegate)
                {
                    _subscriptions.RemoveAt(i);
                    break;
                }
            }
            EventBusSystem.Unsubscribe(handler);
        }

        /// <summary>Publish an event with automatic source tracking.</summary>
        protected void Publish<TEvent>(TEvent eventToPublish, 
            [System.Runtime.CompilerServices.CallerMemberName] string caller = "",
            [System.Runtime.CompilerServices.CallerFilePath] string file = "") where TEvent : class
        {
            if (eventToPublish is GameEvent gameEvent)
            {
                gameEvent.Source = Path.GetFileNameWithoutExtension(file);
                gameEvent.SourceMember = caller;
                gameEvent.Timestamp = DateTime.UtcNow;
            }
            
            if (GetLogLevel() >= EventBusLogLevel.Verbose)
            {
                Debug.Log($"[EventBus] {Path.GetFileNameWithoutExtension(file)}.{caller} published {typeof(TEvent).Name}");
            }
            
            EventBusSystem.Publish(eventToPublish);
        }

        private bool IsSubscribed<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            foreach (var (type, existingHandler) in _subscriptions)
            {
                if (type == eventType && existingHandler == handler as Delegate) return true;
            }
            return false;
        }

        private void UnsubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Unsubscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
            _subscriptions.Clear();
        }

        private void ResubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Subscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
        }

        protected virtual void OnEnable() => ResubscribeAll();
        protected virtual void OnDisable() => UnsubscribeAll();
        protected virtual void OnDestroy() => UnsubscribeAll();
    }
#endregion

#region Event Bus Subscriber Pure (For Pure C# Classes)
    /// <summary>
    /// Base class for pure C# classes that need to subscribe to EventBusSystem events.
    /// Implements IDisposable for manual cleanup.
    /// </summary>
    public abstract class EventBusSubscriberPure : IDisposable
    {
        private List<(Type type, Delegate handler)> _subscriptions = new();
        
        /// <summary>Subscribe to an event. Automatically tracks for cleanup.</summary>
        protected void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;
            
            Type eventType = typeof(TEvent);
            if (IsSubscribed(handler))
            {
                Debug.LogWarning($"[EventBusSubscriberPure] Already subscribed to {eventType.Name}. Skipping duplicate.");
                return;
            }
            
            EventBusSystem.Subscribe(handler);
            _subscriptions.Add((eventType, handler));
        }
        
        /// <summary>Unsubscribe from a specific event.</summary>
        protected void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;
            
            Type eventType = typeof(TEvent);
            for (int i = 0; i < _subscriptions.Count; i++)
            {
                if (_subscriptions[i].type == eventType && _subscriptions[i].handler == handler as Delegate)
                {
                    _subscriptions.RemoveAt(i);
                    break;
                }
            }
            EventBusSystem.Unsubscribe(handler);
        }
        
        /// <summary>Publish an event (no source tracking for pure C# classes).</summary>
        protected void Publish<TEvent>(TEvent eventToPublish) where TEvent : class
        {
            EventBusSystem.Publish(eventToPublish);
        }
        
        private bool IsSubscribed<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            Type eventType = typeof(TEvent);
            foreach (var (type, existingHandler) in _subscriptions)
            {
                if (type == eventType && existingHandler == handler as Delegate) return true;
            }
            return false;
        }
        
        /// <summary>Unsubscribe from all events. Call this in Dispose().</summary>
        protected virtual void UnsubscribeAll()
        {
            foreach (var (type, handler) in _subscriptions)
            {
                if (handler == null) continue;
                typeof(EventBusSystem).GetMethod("Unsubscribe")?.MakeGenericMethod(type).Invoke(null, new object[] { handler });
            }
            _subscriptions.Clear();
        }
        
        /// <summary>Dispose of event subscriptions. Override to add custom cleanup.</summary>
        public virtual void Dispose()
        {
            UnsubscribeAll();
        }
    }
#endregion
}