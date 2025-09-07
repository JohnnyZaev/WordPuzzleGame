using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;
using WPG.Runtime.Utilities.AddressablesController;
using WPG.Runtime.Utilities.Loading;
using WPG.Runtime.Utilities.SaveController;

namespace WPG.Runtime.Bootstrap
{
    public sealed class BootstrapScope : LifetimeScope
    {
        [SerializeField] private LoadingView _loadingViewPrefab;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_loadingViewPrefab);
            
            builder.Register<LoadingService>(Lifetime.Singleton);
            builder.Register<ILoadingController, LoadingController>(Lifetime.Singleton);
            builder.Register<IAddressablesController, AddressablesController>(Lifetime.Singleton);
            builder.Register<ISaveController, SaveController>(Lifetime.Singleton);

            builder.RegisterEntryPoint<BootstrapFlow>();
        }
    }
}