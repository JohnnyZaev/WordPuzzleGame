using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using WPG.Runtime.UI.View;
using WPG.Runtime.Utilities.Logging;

namespace WPG.Runtime.Bootstrap
{
    public class LoadingView : View
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _progressBar;
        
        private Tween _fadeTween;
        private Tween _progressTween;
        private readonly CancellationTokenSource _fadeCancellationTokenSource = new CancellationTokenSource();

        public override UniTask Show()
        {
            if (gameObject.activeSelf)
            {
                Log.Bootstrap.Info("Loading screen is already visible.");
                return UniTask.CompletedTask;
            }

            _fadeTween?.Kill();

            gameObject.SetActive(true);
            _fadeTween = _canvasGroup.DOFade(1f, 0.5f)
                .SetAutoKill(true)
                .Play();
            return UniTask.CompletedTask;
        }

        public override async UniTask Hide()
        {
            _fadeTween?.Kill();

            _progressBar.fillAmount = 1;
            
            _fadeTween = _canvasGroup.DOFade(0f, 0.5f)
                .SetAutoKill(true)
                .Play();
            await _fadeTween.ToUniTask(TweenCancelBehaviour.Kill, _fadeCancellationTokenSource.Token);
            
            gameObject.SetActive(false);
        }

        public void ReportLoadingProgress(int stepNum, int allSteps)
        {
            float deltaPerStep = 1f / allSteps;
            float start = stepNum * deltaPerStep;
            
            _progressBar.fillAmount = start;
            _progressTween?.Kill();
            _progressTween = _progressBar.DOFillAmount(start + deltaPerStep, 0.5f)
                .SetAutoKill(true)
                .Play();
        }

        public void ResetProgress()
        {
            _canvasGroup.alpha = 0f;
            _progressBar.fillAmount = 0f;
            _progressTween?.Kill();
            gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            _fadeCancellationTokenSource?.Dispose();
        }
    }
}