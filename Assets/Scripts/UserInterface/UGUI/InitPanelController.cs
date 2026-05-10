using System.Collections;
using Coordinators;
using UnityEngine;
using Zenject;

namespace UserInterface.UGUI
{
    public class InitPanelController : MonoBehaviour
    {
        [Inject] private GameFlowCoordinator _gameFlowCoordinator;
        
        private IEnumerator Start()
        {
            // I stripped out all backend integrations for this example demo. the game flow is initiated after all the bootloader systems have been initialized.
            // Here this is simulated by waiting one frame, letting the game systems setup their dependencies to primarily the event system,
            // and then starting the game flow. 
            yield return null;

            Initialize();
        }

        private void Initialize()
        {
            _gameFlowCoordinator.Initialize();
        }
    }
}
