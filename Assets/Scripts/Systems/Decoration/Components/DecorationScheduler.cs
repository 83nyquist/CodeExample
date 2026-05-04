using System;
using System.Collections;
using System.Collections.Generic;
using Systems.Decoration.Interfaces;
using Systems.Grid.Components;
using UnityEngine;

namespace Systems.Decoration.Components
{
    public class DecorationScheduler : IDecorationScheduler
    {
        private readonly DecoratorFactory _factory;
        private readonly float _maxMsPerFrame;
        private readonly Dictionary<TileData, TileDecorator> _activeDecorators = new();

        public bool IsProcessing { get; private set; }
        public event Action OnProcessingFinished;

        public DecorationScheduler(DecoratorFactory factory, float maxMsPerFrame)
        {
            _factory = factory;
            _maxMsPerFrame = maxMsPerFrame;
        }

        public IEnumerator ProcessQueues(IEnumerable<TileData> toShow, IEnumerable<TileData> toHide)
        {
            IsProcessing = true;
            float budgetSeconds = _maxMsPerFrame / 1000f;

            Queue<TileData> showQueue = new Queue<TileData>(toShow);
            Queue<TileData> hideQueue = new Queue<TileData>(toHide);

            while (showQueue.Count > 0 || hideQueue.Count > 0)
            {
                float startTime = Time.realtimeSinceStartup;

                // 1. Process Hides (Priority: Free up resources)
                while (hideQueue.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

                    TileData data = hideQueue.Dequeue();
                    if (_activeDecorators.TryGetValue(data, out TileDecorator decorator))
                    {
                        _activeDecorators.Remove(data);
                        _factory.ReturnTileDecorator(decorator);
                    }
                }

                // 2. Process Shows
                while (showQueue.Count > 0)
                {
                    if (Time.realtimeSinceStartup - startTime > budgetSeconds) break;

                    TileData data = showQueue.Dequeue();
                    
                    // Defensive check: don't double-spawn if already active
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

                yield return null;
            }

            IsProcessing = false;
            OnProcessingFinished?.Invoke();
        }
    }
}