using VContainer;
using VContainer.Unity;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.WordPuzzle
{
    public class GameplayScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<WordPuzzleFlow>();
        }
    }

    public class WordPuzzleFlow : IStartable
    {
        public void Start()
        {
            Log.Gameplay.Info("WordPuzzleFlow started");
        }
    }
}