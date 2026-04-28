using Audio;
using Character;
using Input;
using Systems.Decoration;
using Systems.Grid;
using UserInterface;
using Zenject;

namespace Systems.DependencyInjection
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<AxialHexGrid>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<WorldDecorator>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<DecoratorFactory>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<CharacterController>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<CharacterPathfinding>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<CharacterMover>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<MouseInput>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<InputHandler>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<UIController>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<AudioManager>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<CharacterAnimationEvents>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<OutputAggregator>().FromComponentInHierarchy().AsCached().NonLazy();
        }
    }
}
