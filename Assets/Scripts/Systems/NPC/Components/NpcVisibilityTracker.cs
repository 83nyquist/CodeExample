using System;
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

        public void Process(NativeArray<NpcData> npcs, float dt)
        {
            if (!npcs.IsCreated) return;

            _timer += dt;
            if (_timer < _interval) return;
            _timer = 0;

            int count = 0;
            for (int i = 0; i < npcs.Length; i++)
                if (npcs[i].IsVisible) count++;

            if (count != _lastCount)
            {
                _lastCount = count;
                OnCountChanged?.Invoke(count);
            }
        }
    }
}
