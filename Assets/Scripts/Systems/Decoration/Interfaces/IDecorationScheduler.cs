using System;
using System.Collections;
using System.Collections.Generic;
using Systems.Grid.Components;

namespace Systems.Decoration.Interfaces
{
    public interface IDecorationScheduler
    {
        bool IsProcessing { get; }
        event Action OnProcessingFinished;
        IEnumerator ProcessQueues(IEnumerable<TileData> toShow, IEnumerable<TileData> toHide);
    }
}