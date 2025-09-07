using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace WPG.Runtime.Gameplay
{
    public class GameplayScope : LifetimeScope
    {
        [SerializeField] private GameplayReferences _gameplayReferences;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_gameplayReferences);
            
            builder.Register<LevelController>(Lifetime.Scoped);
            
            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}