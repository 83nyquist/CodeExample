using System;
using System.Collections.Generic;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.NonPlayerCharacters.Structs;
using Unity.Collections;

namespace Systems.NonPlayerCharacters.Components
{
    public class NpcVisibilityTracker
    {
        private float _timer;
        private readonly float _interval;
        private int _lastCount = -1;

        /// <summary>
        /// Event triggered when the number of visible NPCs changes.
        /// </summary>
        public event Action<int> OnCountChanged;

        /// <summary>
        /// Initializes the tracker with a specific update interval to throttle visibility calculations.
        /// </summary>
        public NpcVisibilityTracker(float interval) => _interval = interval;

        /// <summary>
        /// Processes NPC data against the current vision set and triggers events if the visible count changes.
        /// </summary>
        public void Process(NativeArray<NpcData> npcs, AxialHexGrid grid, HashSet<TileData> visionSet, float dt)
        {
            if (!npcs.IsCreated) return;

            _timer += dt;
            if (_timer < _interval) return;
            _timer = 0;

            int count = 0;
            for (int i = 0; i < npcs.Length; i++)
            {
                var tile = grid.GetTile(npcs[i].Position.x, npcs[i].Position.y);
                if (tile != null && visionSet.Contains(tile)) 
                    count++;
            }

            if (count != _lastCount)
            {
                _lastCount = count;
                OnCountChanged?.Invoke(count);
            }
        }
    }
}
