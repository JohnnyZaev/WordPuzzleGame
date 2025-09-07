using UnityEngine;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Data
{
    [CreateAssetMenu(fileName = "WordData", menuName = "WPG/Word Data", order = 2)]
    public class WordData : ScriptableObject
    {
        [SerializeField] private string _word;
        
        public string Word => _word;
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_word))
            {
                return;
            }

            _word = _word.ToUpper();
            if (_word.Length != 6)
            {
                Log.Gameplay.Warning($"Word '{name}' should be exactly 6 letters long. Current length: {_word.Length}");
            }
        }
    }
}