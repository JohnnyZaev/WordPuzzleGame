using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Gameplay.Views
{
    public class LetterClusterView : View, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _lettersText;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalAlpha = Color.white;
        [SerializeField] private Color _usedAlpha = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float _dragScale = 1.1f;
        
        private readonly CompositeDisposable _disposables = new();
        private LetterClusterViewModel _viewModel;
        private GameFieldView _gameFieldView;
        private RectTransform _rectTransform;
        private Canvas _canvas;
        
        private Vector2 _originalPosition;
        private Vector3 _originalScale;
        private bool _isDragging;
        private int _originalSiblingIndex;
        
        public int ClusterId => _viewModel?.Id ?? -1;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
            _originalScale = transform.localScale;
        }
        
        public void Initialize(LetterClusterViewModel viewModel, GameFieldView gameFieldView)
        {
            _viewModel = viewModel;
            _gameFieldView = gameFieldView;
            _originalPosition = _rectTransform.anchoredPosition;
            _originalSiblingIndex = transform.GetSiblingIndex();
            
            // Subscribe to ViewModel properties
            _viewModel.Letters
                .Subscribe(letters => 
                {
                    if (_lettersText != null)
                    {
                        _lettersText.text = letters;
                    }
                })
                .AddTo(_disposables);
                
            _viewModel.FrameColor
                .Subscribe(color => 
                {
                    if (_frameImage != null)
                    {
                        _frameImage.color = color;
                    }
                })
                .AddTo(_disposables);
                
            _viewModel.Alpha
                .Subscribe(UpdateAlpha)
                .AddTo(_disposables);
                
            _viewModel.IsUsed
                .Subscribe(UpdateUsedState)
                .AddTo(_disposables);
                
            _viewModel.IsInteractable
                .Subscribe(UpdateInteractableState)
                .AddTo(_disposables);
                
            _viewModel.IsDragging
                .Subscribe(UpdateDraggingState)
                .AddTo(_disposables);
                
            _viewModel.Position
                .Subscribe(UpdatePosition)
                .AddTo(_disposables);
        }
        
        private void UpdateAlpha(float alpha)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
        }
        
        private void UpdateUsedState(bool isUsed)
        {
            // Visual feedback for used state
            var targetColor = isUsed ? _usedAlpha : _normalAlpha;
            if (_backgroundImage != null)
            {
                _backgroundImage.color = targetColor;
            }
        }
        
        private void UpdateInteractableState(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
                _canvasGroup.blocksRaycasts = isInteractable;
            }
        }
        
        private void UpdateDraggingState(bool isDragging)
        {
            _isDragging = isDragging;
            
            if (isDragging)
            {
                transform.localScale = _originalScale * _dragScale;
                
                // Bring to the front
                transform.SetAsLastSibling();
            }
            else
            {
                transform.localScale = _originalScale;
                
                // Return to the original position if not placed
                if (_viewModel.IsUsed.Value)
                {
                    return;
                }

                _rectTransform.anchoredPosition = _originalPosition;
                // Restore the original sibling index to fix z-order layering
                transform.SetSiblingIndex(_originalSiblingIndex);
            }
        }
        
        private void UpdatePosition(Vector2 position)
        {
            if (_isDragging)
            {
                // Convert screen position to a local position
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rectTransform.parent as RectTransform, position, _canvas.worldCamera, out var localPoint);
                
                _rectTransform.anchoredPosition = localPoint;
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_viewModel.IsInteractable.Value) return;
            
            // Visual feedback for press
            if (_backgroundImage != null)
            {
                _backgroundImage.color = Color.gray;
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_viewModel.IsInteractable.Value) return;
            
            // Restore normal color
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _normalAlpha;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_viewModel.IsInteractable.Value) return;
            
            _viewModel.StartDragCommand.Execute(eventData.position);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            _viewModel.DragCommand.Execute(eventData.position);
            
            // Check for hover over word slots
            _ = _gameFieldView.GetWordSlotIndexAtPosition(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            // Check if dropped on a word slot
            int targetSlot = _gameFieldView.GetWordSlotIndexAtPosition(eventData.position);
            
            if (targetSlot >= 0 && _viewModel.CanBePlacedInSlot(targetSlot))
            {
                _viewModel.DropOnWordSlotCommand.Execute(targetSlot);
            }
            else if (_viewModel.IsUsed.Value)
            {
                // If the cluster was already used and dragged outside valid zones, remove it
                _viewModel.RemoveFromSlotCommand.Execute(Unit.Default);
            }
            else
            {
                _viewModel.ReturnToOriginCommand.Execute(Unit.Default);
            }
        }
        
        public Vector2 GetOriginPosition()
        {
            return RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, 
                _rectTransform.TransformPoint(_originalPosition));
        }
        
        public Vector2 GetCurrentScreenPosition()
        {
            return RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, _rectTransform.position);
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