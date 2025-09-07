using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using VContainer.Unity;

namespace WPG.Runtime.Menu
{
    public class MenuScope : LifetimeScope
    {
        [SerializeField] private AssetReference _menuView;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_menuView);
            
            builder.RegisterEntryPoint<MenuFlow>();
        }
    }
}