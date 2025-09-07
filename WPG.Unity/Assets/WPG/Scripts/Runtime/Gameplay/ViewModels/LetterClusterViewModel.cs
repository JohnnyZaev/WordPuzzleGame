using System;
using UnityEngine;
using R3;

namespace WPG.Runtime.Gameplay.ViewModels
{
    public class LetterClusterViewModel : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();
        private readonly LetterClusterItem _clusterItem;
        private readonly GameViewModel _gameViewModel;
        
        // Public reactive properties for UI binding
        public readonly ReactiveProperty<string> Letters = new(string.Empty);
        public readonly ReactiveProperty<Color> FrameColor = new(Color.white);
        public readonly ReactiveProperty<bool> IsUsed = new();
        public readonly ReactiveProperty<int> UsedInWordSlot = new(-1);
        public readonly ReactiveProperty<bool> IsDragging = new();
        public readonly ReactiveProperty<Vector2> Position = new();
        
        // UI-specific properties
        public readonly ReactiveProperty<float> Alpha = new(1.0f);
        public readonly ReactiveProperty<bool> IsInteractable = new(true);
        
        // Commands
        public readonly ReactiveCommand<Vector2> StartDragCommand = new();
        public readonly ReactiveCommand<Vector2> DragCommand = new();
        public readonly ReactiveCommand<int> DropOnWordSlotCommand = new();
        public readonly ReactiveCommand<Unit> ReturnToOriginCommand = new();
        public readonly ReactiveCommand<Unit> RemoveFromSlotCommand = new();
        
        public int Id => _clusterItem.Id;
        
        public LetterClusterViewModel(LetterClusterItem clusterItem, GameViewModel gameViewModel)
        {
            _clusterItem = clusterItem;
            _gameViewModel = gameViewModel;
            
            // Bind to cluster item properties
            Letters.Value = _clusterItem.Letters;
            
            _clusterItem.IsUsed
                .Subscribe(used => 
                {
                    IsUsed.Value = used;
                    Alpha.Value = used ? 0.5f : 1.0f;
                    // Keep clusters interactable even when used so they can be dragged out
                    IsInteractable.Value = true;
                })
                .AddTo(_disposables);
                
            _clusterItem.UsedInWordSlot
                .Subscribe(slot => UsedInWordSlot.Value = slot)
                .AddTo(_disposables);
            
            // Parse frame color
            if (ColorUtility.TryParseHtmlString(_clusterItem.FrameColor, out Color color))
            {
                FrameColor.Value = color;
            }
            
            // Subscribe to commands
            StartDragCommand
                .Subscribe(StartDrag)
                .AddTo(_disposables);
                
            DragCommand
                .Where(_ => IsDragging.Value)
                .Subscribe(UpdateDragPosition)
                .AddTo(_disposables);
                
            DropOnWordSlotCommand
                .Where(_ => IsDragging.Value)
                .Subscribe(TryDropOnWordSlot)
                .AddTo(_disposables);
                
            ReturnToOriginCommand
                .Subscribe(_ => ReturnToOrigin())
                .AddTo(_disposables);
                
            RemoveFromSlotCommand
                .Where(_ => IsUsed.Value)
                .Subscribe(_ => RemoveFromSlot())
                .AddTo(_disposables);
        }
        
        private void StartDrag(Vector2 startPosition)
        {
            IsDragging.Value = true;
            Position.Value = startPosition;
            Alpha.Value = 0.8f;
        }
        
        private void UpdateDragPosition(Vector2 position)
        {
            Position.Value = position;
        }
        
        private void TryDropOnWordSlot(int wordSlotIndex)
        {
            bool placed = _gameViewModel.TryPlaceCluster(Id, wordSlotIndex);
            
            if (placed)
            {
                // Successfully placed
                IsDragging.Value = false;
                Alpha.Value = 0.5f;
            }
            else
            {
                // Failed to place, return to origin
                ReturnToOrigin();
            }
        }
        
        private void ReturnToOrigin()
        {
            IsDragging.Value = false;
            Alpha.Value = IsUsed.Value ? 0.5f : 1.0f;
            // The view will reset position
        }
        
        private void RemoveFromSlot()
        {
            bool removed = _gameViewModel.TryRemoveCluster(Id);
            
            if (removed)
            {
                Alpha.Value = 1.0f;
                IsInteractable.Value = true;
            }
        }
        
        public bool CanBePlacedInSlot(int wordSlotIndex)
        {
            if (IsUsed.Value || wordSlotIndex < 0)
            {
                return false;
            }
            
            // Check with a game view model if this placement is valid
            var wordSlots = _gameViewModel.WordSlots.Value;
            if (wordSlots != null && wordSlotIndex < wordSlots.Count)
            {
                return wordSlots[wordSlotIndex].CanAcceptCluster(_clusterItem);
            }
            
            return false;
        }
        
        public void Dispose()
        {
            _disposables?.Dispose();
            Letters?.Dispose();
            FrameColor?.Dispose();
            IsUsed?.Dispose();
            UsedInWordSlot?.Dispose();
            IsDragging?.Dispose();
            Position?.Dispose();
            Alpha?.Dispose();
            IsInteractable?.Dispose();
            StartDragCommand?.Dispose();
            DragCommand?.Dispose();
            DropOnWordSlotCommand?.Dispose();
            ReturnToOriginCommand?.Dispose();
            RemoveFromSlotCommand?.Dispose();
        }
    }
}