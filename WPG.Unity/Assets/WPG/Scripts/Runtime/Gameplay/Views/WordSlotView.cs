using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Gameplay.Views
{
    public class WordSlotView : View
    {
        [Header("UI References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _dropZone;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        
        private readonly CompositeDisposable _disposables = new();
        private WordSlotViewModel _viewModel;
        private RectTransform _rectTransform;
        private readonly List<LetterClusterView> _placedClusterViews = new();
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public void Initialize(WordSlotViewModel viewModel)
        {
            _viewModel = viewModel;
                
            _viewModel.IsCompleted
                .Subscribe(UpdateVisualState)
                .AddTo(_disposables);
                
            _viewModel.CompletionProgress
                .Subscribe(UpdateProgressVisual)
                .AddTo(_disposables);
                
            Observable.Merge(
                _viewModel.IsCompleted.AsUnitObservable(),
                Observable.EveryUpdate().AsUnitObservable()
            )
            .Subscribe(_ => UpdatePlacedClusterViewsFromCentralizedState())
            .AddTo(_disposables);
        }
        
        private void UpdateVisualState(bool isCompleted)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isCompleted ? _completedColor : _normalColor;
            }
            
            if (isCompleted)
            {
                foreach (var clusterView in _placedClusterViews)
                {
                    clusterView.enabled = false;
                }
            }
        }
        
        private void UpdateProgressVisual(float progress)
        {
            if (_backgroundImage != null)
            {
                var color = Color.Lerp(_normalColor, _completedColor, progress);
                _backgroundImage.color = color;
            }
        }
        
        private void UpdatePlacedClusterViewsFromCentralizedState()
        {
            if (_viewModel != null)
            {
                var currentClusterViews = _viewModel.GetPlacedClusterViews();
                UpdatePlacedClusterViews(currentClusterViews);
            }
        }
        
        private void UpdatePlacedClusterViews(List<LetterClusterView> clusterViews)
        {
            _placedClusterViews.Clear();
            
            if (clusterViews != null)
            {
                _placedClusterViews.AddRange(clusterViews);
                for (int i = _placedClusterViews.Count - 1; i >= 0; i--)
                {
                    if (_placedClusterViews[i].ClusterId == -1)
                    {
                        _placedClusterViews.RemoveAt(i);
                    }
                }
                
                for (int i = 0; i < _placedClusterViews.Count; i++)
                {
                    var clusterView = _placedClusterViews[i];
                    if (clusterView != null)
                    {
                        SetClusterViewParent(clusterView);

                        if (_viewModel != null && _viewModel.IsCompleted.Value)
                        {
                            clusterView.enabled = false;
                        }
                        else
                        {
                            clusterView.enabled = true;
                        }
                    }
                }
            }
        }
        
        private void SetClusterViewParent(LetterClusterView clusterView)
        {
            if (clusterView == null || _dropZone == null) return;
            
            clusterView.transform.SetParent(_dropZone, false);
            
            var rectTransform = clusterView.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
                
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                int index = _placedClusterViews.IndexOf(clusterView);
                if (index >= 0)
                {
                    float clusterWidth = 100f;
                    float spacing = 5f;
                    float totalClusters = _placedClusterViews.Count;
                    
                    float totalWidth = totalClusters * clusterWidth + (totalClusters - 1) * spacing;
                    float startX = -(totalWidth * 0.5f) + (clusterWidth * 0.5f);
                    
                    float xPosition = startX + index * (clusterWidth + spacing);
                    rectTransform.anchoredPosition = new Vector2(xPosition, 0);
                }
                else
                {
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }
        
        public bool ContainsScreenPosition(Vector2 screenPosition)
        {
            if (_dropZone != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dropZone, screenPosition, null, out var localPosition);
                
                return _dropZone.rect.Contains(localPosition);
            }
            
            if (_rectTransform != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform, screenPosition, null, out var localPosition);
                
                return _rectTransform.rect.Contains(localPosition);
            }
            
            return false;
        }
        
        public bool CanAcceptClusterView(LetterClusterView clusterView)
        {
            return _viewModel != null && _viewModel.CanAcceptClusterView(clusterView);
        }
        
        public void AcceptClusterView(LetterClusterView clusterView)
        {
            if (_viewModel != null && clusterView != null)
            {
                _viewModel.PlaceClusterView(clusterView);
            }
        }
        
        public override UniTask Show()
        {
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }
        
        public override UniTask Hide()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
        
        public override void Dispose()
        {
            _disposables?.Dispose();
        }
    }
}