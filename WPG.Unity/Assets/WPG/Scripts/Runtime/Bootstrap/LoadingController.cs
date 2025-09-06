using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WPG.Runtime.Bootstrap
{
    public class LoadingController : ILoadingController
    {
        private LoadingView _loadingView;

        public LoadingController(LoadingView loadingView)
        {
            _loadingView = loadingView;
        }

        public UniTask Load()
        {
            _loadingView = Object.Instantiate(_loadingView.gameObject).GetComponent<LoadingView>();
            _loadingView.ResetProgress();
            Object.DontDestroyOnLoad(_loadingView.gameObject);
            _loadingView.Show();
            return UniTask.CompletedTask;
        }

        public void ReportLoadingProgress(int stepNum, int allSteps)
        {
            _loadingView.ReportLoadingProgress(stepNum, allSteps);
        }

        public void StartLoading()
        {
            _loadingView.Show();
        }
        
        public async UniTask FinishLoading()
        {
            await _loadingView.Hide();
        }
        
        public void Dispose()
        {
            Object.Destroy(_loadingView.gameObject);
        }
    }
}