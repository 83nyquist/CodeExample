using System.Collections.Generic;
using System.Linq;
using Character;
using Systems.Decoration;
using UnityEngine;
using Zenject;

namespace Input
{
    public class InputHandler : MonoBehaviour
    {
        [Inject] private MouseInput _mouseInput;
        [Inject] private CharacterPathfinding _characterPathfinding;
        [Inject] private CharacterMover _characterMover;

        private List<InputLock> _inputLocks = new List<InputLock>();
        
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
            
            _characterMover.TraversePath(_characterPathfinding.currentPath);
        }
        
        private void DrawPath(TileDecorator decorator)
        {
            if (IsInputLocked)
            {
                return;
            }
            
            if (decorator == null)
            {
                _characterPathfinding.ErasePath();
                return;
            }
        
            _characterPathfinding.DrawPath(decorator);
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
