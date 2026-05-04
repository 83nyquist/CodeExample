using System;
using System.Collections.Generic;
using Systems.Grid;
using Systems.Grid.Components;
using Systems.NPC.Structs;
using Unity.Collections;

namespace Systems.NPC.Components
{
    /// <summary>
    /// SRP: Handles visibility logic and event throttling.
    /// </summary>
    public class NpcVisibilityTracker
    {
        private float _timer;
        private readonly float _interval;
        private int _lastCount = -1;
        public event Action<int> OnCountChanged;

        public NpcVisibilityTracker(float interval) => _interval = interval;

        public void Process(NativeArray<NpcData> npcs, AxialHexGrid grid, HashSet<TileData> visionSet, float dt)
        {
            if (!npcs.IsCreated) return;

            _timer += dt;
            if (_timer < _interval) return;
            _timer = 0;

            int count = 0;
            for (int i = 0; i < npcs.Length; i++)
            {
                // We check the grid for the tile at the NPC's position.
                // If that tile is in the vision set, the NPC is logically visible.
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
