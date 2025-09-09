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
        [SerializeField] private Image _backgroundImage;
        
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
        private Vector2 _originalAnchorMin;
        private Vector2 _originalAnchorMax;
        private Vector2 _originalPivot;
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
            _originalAnchorMin = _rectTransform.anchorMin;
            _originalAnchorMax = _rectTransform.anchorMax;
            _originalPivot = _rectTransform.pivot;
            
            _viewModel.Letters
                .Subscribe(letters => 
                {
                    if (_lettersText != null)
                    {
                        _lettersText.text = letters;
                    }
                })
                .AddTo(_disposables);
                
                
            _viewModel.IsUsed
                .Subscribe(UpdateUsedState)
                .AddTo(_disposables);
                
            _viewModel.IsDragging
                .Subscribe(UpdateDraggingState)
                .AddTo(_disposables);
                
            _viewModel.Position
                .Subscribe(UpdatePosition)
                .AddTo(_disposables);
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
        
        private void UpdateDraggingState(bool isDragging)
        {
            _isDragging = isDragging;
            
            if (isDragging)
            {
                transform.localScale = _originalScale * _dragScale;
                
                transform.SetAsLastSibling();
            }
            else
            {
                transform.localScale = _originalScale;
                
                if (!_viewModel.IsUsed.Value)
                {
                    var originalContainer = _gameFieldView?.GetLetterClustersContainer();
                    if (originalContainer != null && transform.parent != originalContainer)
                    {
                        transform.SetParent(originalContainer, false);
                    }
                    
                    _rectTransform.anchorMin = _originalAnchorMin;
                    _rectTransform.anchorMax = _originalAnchorMax;
                    _rectTransform.pivot = _originalPivot;
                    
                    _rectTransform.anchoredPosition = _originalPosition;
                    transform.SetSiblingIndex(_originalSiblingIndex);
                }
            }
        }
        
        private void UpdatePosition(Vector2 position)
        {
            if (!_isDragging)
            {
                return;
            }

            var canvasRectTransform = _canvas.GetComponent<RectTransform>();
            if (canvasRectTransform == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, position, _canvas.worldCamera, out var localPoint);
                    
            var parentRect = _rectTransform.parent as RectTransform;
            if (parentRect != null)
            {
                var worldPoint = canvasRectTransform.TransformPoint(localPoint);
                var parentLocalPoint = parentRect.InverseTransformPoint(worldPoint);
                _rectTransform.anchoredPosition = parentLocalPoint;
            }
            else
            {
                _rectTransform.anchoredPosition = localPoint;
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_viewModel.IsInteractable.Value) return;
            
            if (_backgroundImage != null)
            {
                _backgroundImage.color = Color.gray;
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_viewModel.IsInteractable.Value) return;
            
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
            
            _ = _gameFieldView.GetWordSlotIndexAtPosition(eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;
            
            int targetSlot = _gameFieldView.GetWordSlotIndexAtPosition(eventData.position);
            
            if (targetSlot >= 0)
            {
                var wordSlotViews = _gameFieldView.GetWordSlotViews();
                if (targetSlot < wordSlotViews.Count)
                {
                    var wordSlotView = wordSlotViews[targetSlot];
                    if (wordSlotView.CanAcceptClusterView(this))
                    {
                        if (_viewModel.IsUsed.Value)
                        {
                            _viewModel.RemoveFromSlotCommand.Execute(Unit.Default);
                        }
                        
                        wordSlotView.AcceptClusterView(this);
                        _viewModel.IsDragging.Value = false;
                        return;
                    }
                }
            }
            
            if (_viewModel.IsUsed.Value)
            {
                _viewModel.RemoveFromSlotCommand.Execute(Unit.Default);
            }
            else
            {
                _viewModel.ReturnToOriginCommand.Execute(Unit.Default);
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