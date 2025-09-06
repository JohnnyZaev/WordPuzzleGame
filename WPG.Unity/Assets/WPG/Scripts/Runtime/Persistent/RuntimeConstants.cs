using UnityEngine.SceneManagement;

namespace WPG.Runtime.Persistent
{
    public static class RuntimeConstants
    {
        public static class Scenes
        {
            public static readonly int Bootstrap = SceneUtility.GetBuildIndexByScenePath("0. Bootstrap");
            public static readonly int Gameplay = SceneUtility.GetBuildIndexByScenePath("1. Gameplay");
        }
    }
}