using System.Collections;
using Coordinators;
using UnityEngine;
using Zenject;

namespace UserInterface.UGUI
{
    public class InitPanelController : MonoBehaviour
    {
        [Inject] private GameFlowCoordinator _gameFlowCoordinator;
        
        /// <summary>
        /// Simulates a bootloader delay before initiating the game flow.
        /// </summary>
        private IEnumerator Start()
        {
            yield return null;
            Initialize();
        }

        /// <summary>
        /// Triggers the initialization of the game flow coordinator.
        /// </summary>
        private void Initialize()
        {
            _gameFlowCoordinator.Initialize();
        }
    }
}
