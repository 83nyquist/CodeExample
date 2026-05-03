using Audio;
using Character;
using Input;
using NPC;
using Systems.Decoration;
using Systems.Grid;
using UserInterface;
using UserInterface.UGUI;
using UserInterface.UIToolkit;
using Vanguard;

namespace Systems.DependencyInjection
{
    public class MonoInstaller : Zenject.MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<AxialHexGrid>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<WorldDecorator>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<DecoratorFactory>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<NpcManager>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<GenerationProgressTracker>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<VanguardController>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<AStarPathfinding>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<VanguardMover>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<MouseInput>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<InputHandler>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<UiManager>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<UIController>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<UiLabels>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<DebugDrawer>().FromComponentInHierarchy().AsCached().NonLazy();
            Container.Bind<LoadingPanelController>().FromComponentInHierarchy().AsCached().NonLazy();
            
            Container.Bind<AudioManager>().FromComponentInHierarchy().AsCached().NonLazy();
        }
    }
}
