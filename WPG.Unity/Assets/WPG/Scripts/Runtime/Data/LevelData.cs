using System;

namespace WPG.Runtime.Data
{
    [Serializable]
    public class LevelData
    {
        public int levelNumber;
        public string levelName;
        public string[] targetWords;
        public LetterClusterData[] letterClusters;
    }

    [Serializable]
    public class LetterClusterData
    {
        public string letters;
        public string frameColor;
    }
}