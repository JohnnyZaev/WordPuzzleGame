using System;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Utilities.Loading;

namespace WPG.Runtime.Bootstrap
{
    public interface ILoadingController : ILoadUnit, IDisposable
    {
        public void ReportLoadingProgress(int stepNum, int allSteps);
        public void StartLoading();
        public UniTask FinishLoading();
    }
}