using System;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Gameplay.Views
{
    public class WordSlotView : View
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _displayText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private RectTransform _dropZone;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _hoverColor = Color.yellow;
        
        private readonly CompositeDisposable _disposables = new();
        private WordSlotViewModel _viewModel;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }
        
        public void Initialize(WordSlotViewModel viewModel)
        {
            _viewModel = viewModel;
            
            // Subscribe to ViewModel properties
            _viewModel.DisplayText
                .Subscribe(text => 
                {
                    if (_displayText != null)
                    {
                        _displayText.text = text;
                    }
                })
                .AddTo(_disposables);
                
            _viewModel.IsCompleted
                .Subscribe(UpdateVisualState)
                .AddTo(_disposables);
                
            _viewModel.CompletionProgress
                .Subscribe(UpdateProgressVisual)
                .AddTo(_disposables);
        }
        
        private void UpdateVisualState(bool isCompleted)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = isCompleted ? _completedColor : _normalColor;
            }
            
            // Add completion animation or effects here if needed
            if (isCompleted)
            {
                // Could add particle effects, sound, etc.
            }
        }
        
        private void UpdateProgressVisual(float progress)
        {
            // Visual feedback for completion progress
            if (_backgroundImage != null)
            {
                var color = Color.Lerp(_normalColor, _completedColor, progress);
                _backgroundImage.color = color;
            }
        }
        
        public bool ContainsScreenPosition(Vector2 screenPosition)
        {
            if (_dropZone != null)
            {
                // Convert screen position to local position relative to drop zone
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dropZone, screenPosition, null, out var localPosition);
                
                return _dropZone.rect.Contains(localPosition);
            }
            
            if (_rectTransform != null)
            {
                // Fallback to the main rect transform
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform, screenPosition, null, out var localPosition);
                
                return _rectTransform.rect.Contains(localPosition);
            }
            
            return false;
        }
        
        public Vector2 GetCenterPosition()
        {
            if (_dropZone != null)
            {
                return RectTransformUtility.WorldToScreenPoint(null, _dropZone.position);
            }
            
            if (_rectTransform != null)
            {
                return RectTransformUtility.WorldToScreenPoint(null, _rectTransform.position);
            }
            
            return Vector2.zero;
        }
        
        public void SetHoverState(bool isHovering)
        {
            if (_backgroundImage != null && !_viewModel.IsCompleted.Value)
            {
                _backgroundImage.color = isHovering ? _hoverColor : _normalColor;
            }
        }
        
        public bool CanAcceptDrop()
        {
            return _viewModel != null && !_viewModel.IsCompleted.Value;
        }
        
        public int GetSlotIndex()
        {
            return _viewModel?.Index ?? -1;
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