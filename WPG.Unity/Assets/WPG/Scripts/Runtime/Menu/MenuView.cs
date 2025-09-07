using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Menu
{
    public class MenuView : View
    {
        public CanvasGroup _canvasGroup;
        public Button _playButton;
        public TMP_Text _currentLevelText;
        public TMP_Text _completedLevelsText;
    
        public override UniTask Show()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask Hide()
        {
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
        }
    }
}
