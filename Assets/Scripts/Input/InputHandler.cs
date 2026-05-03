using System.Collections.Generic;
using System.Linq;
using Systems.Decoration.Components;
using Systems.Grid.Pathfinding;
using UnityEngine;
using Vanguard;
using Zenject;

namespace Input
{
    public class InputHandler : MonoBehaviour
    {
        [Inject] private MouseInput _mouseInput;
        [Inject] private AStarPathfinding _aStarPathfinding;
        [Inject] private VanguardMover _vanguardMover;

        private readonly List<InputLock> _inputLocks = new List<InputLock>();
        
        private bool IsInputLocked => _inputLocks.Any(inputLock => inputLock.IsLocked);
        
        private void Awake()
        {
            _mouseInput.OnTileDecoratorPointerUp += MoveTo;
            _mouseInput.OnTileDecoratorPointerDown += DrawPath;
            _mouseInput.OnTileDecoratorDrag += DrawPath;
        }

        private void OnDestroy()
        {
            _mouseInput.OnTileDecoratorPointerUp -= MoveTo;
            _mouseInput.OnTileDecoratorPointerDown -= DrawPath;
            _mouseInput.OnTileDecoratorDrag -= DrawPath;
        }
        
        private void MoveTo(TileDecorator decorator)
        {
            if (decorator == null || IsInputLocked)
            {
                return;
            }
            
            _vanguardMover.TraversePath(_aStarPathfinding.currentPath);
        }
        
        private void DrawPath(TileDecorator decorator)
        {
            if (IsInputLocked)
            {
                return;
            }
            
            if (decorator == null)
            {
                _aStarPathfinding.ErasePath();
                return;
            }
        
            _aStarPathfinding.DrawPath(decorator);
        }

        public InputLock RegisterInputLock(MonoBehaviour obj)
        {
            InputLock inputLock = new InputLock();
            _inputLocks.Add(inputLock);
            return inputLock;
        }
    }

    public class InputLock
    {
        public bool IsLocked;
    }
}
