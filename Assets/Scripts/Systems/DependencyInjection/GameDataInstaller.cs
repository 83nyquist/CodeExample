using Data;
using UnityEngine;
using Zenject;

namespace Systems.DependencyInjection
{
    [CreateAssetMenu(fileName = "GameDataInstaller", menuName = "Installers/Game Data Installer")]
    public class GameDataInstaller : ScriptableObjectInstaller<GameDataInstaller>
    {
        public GameSettings gameSettings;
        public PlayerSettings playerSettings;

        public override void InstallBindings()
        {
            // Bind the entire settings object
            Container.BindInstance(gameSettings).AsSingle();
            Container.BindInstance(playerSettings).AsSingle();
        }
    }
}
