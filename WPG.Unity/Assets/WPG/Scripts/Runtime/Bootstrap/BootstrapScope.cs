using UnityEngine;
using VContainer;
using VContainer.Unity;
using WPG.Runtime.Utilities.AddressablesController;
using WPG.Runtime.Utilities.Loading;

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

            builder.RegisterEntryPoint<BootstrapFlow>();
        }
    }
}