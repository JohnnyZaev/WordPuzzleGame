using UnityEngine.SceneManagement;

namespace WPG.Runtime.Persistent
{
    public static class RuntimeConstants
    {
        public static class Scenes
        {
            public static readonly int Bootstrap = SceneUtility.GetBuildIndexByScenePath("0. Bootstrap");
            public static readonly int Menu = SceneUtility.GetBuildIndexByScenePath("1. Menu");
            public static readonly int Gameplay = SceneUtility.GetBuildIndexByScenePath("2. Gameplay");
        }
        public static class DTO
        {
            public static readonly string Menu = "MenuData";
        }
    }
}