using System;
using System.Collections;
using System.Collections.Generic;
using Systems.Decoration.Interfaces;
using Systems.EventBus.Interfaces;
using Systems.EventBus.Events;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Decoration.Components
{
    public class DecorationScheduler : IDecorationScheduler
    {
        private readonly DecoratorFactory _factory;
        private readonly float _maxMsPerFrame;
        private readonly IEventBus _eventBus;
        private readonly Dictionary<TileData, TileDecorator> _activeDecorators = new();

        public bool IsProcessing { get; private set; }
        public event Action OnProcessingFinished;

        public DecorationScheduler(DecoratorFactory factory, float maxMsPerFrame, IEventBus eventBus)
        {
            _factory = factory;
            _maxMsPerFrame = maxMsPerFrame;
            _eventBus = eventBus;
        }

        public IEnumerator ProcessQueues(IEnumerable<TileData> toShow, IEnumerable<TileData> toHide, bool reportProgress)
        {
            IsProcessing = true;
            float budgetSeconds = _maxMsPerFrame / 1000f;

            Queue<TileData> showQueue = new Queue<TileData>(toShow);
            Queue<TileData> hideQueue = new Queue<TileData>(toHide);
            int workDoneInFrame = 0;

            while (showQueue.Count > 0 || hideQueue.Count > 0)
            {
                float startTime = Time.realtimeSinceStartup;
                workDoneInFrame = 0;

                while (hideQueue.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

                    TileData data = hideQueue.Dequeue();
                    workDoneInFrame++;
                    
                    if (_activeDecorators.TryGetValue(data, out TileDecorator decorator))
                    {
                        _activeDecorators.Remove(data);
                        _factory.ReturnTileDecorator(decorator);
                    }
                }

                while (showQueue.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

                    TileData data = showQueue.Dequeue();
                    workDoneInFrame++;
                    
                    if (data != null && !_activeDecorators.ContainsKey(data))
                    {
                        TileDecorator decorator = _factory.GetTileDecorator(data);
                        if (decorator != null)
                        {
                            data.SetDecorator(decorator);
                            _activeDecorators[data] = decorator;
                        }
                    }
                }

                if (reportProgress && workDoneInFrame > 0)
                {
                    _eventBus.Publish(new ReportWorkProgressRequest(workDoneInFrame, 0));
                }
                
                yield return null;
            }

            IsProcessing = false;
            OnProcessingFinished?.Invoke();
        }
    }
}
