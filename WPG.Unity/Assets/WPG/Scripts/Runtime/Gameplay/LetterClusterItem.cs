using System;
using R3;
using WPG.Runtime.Data;

namespace WPG.Runtime.Gameplay
{
    public class LetterClusterItem : IDisposable
    {
        public int Id { get; }
        public string Letters { get; }
        public string FrameColor { get; }
        
        public readonly ReactiveProperty<bool> IsUsed = new();
        public readonly ReactiveProperty<int> UsedInWordSlot = new(-1);
        
        public LetterClusterItem(int id, LetterClusterData clusterData)
        {
            Id = id;
            Letters = clusterData.letters?.ToUpper() ?? string.Empty;
            FrameColor = clusterData.frameColor ?? "#FFFFFFFF";
        }
        
        public void SetUsed(int wordSlotIndex)
        {
            IsUsed.Value = true;
            UsedInWordSlot.Value = wordSlotIndex;
        }
        
        public void SetUnused()
        {
            IsUsed.Value = false;
            UsedInWordSlot.Value = -1;
        }
        
        public void Dispose()
        {
            IsUsed?.Dispose();
            UsedInWordSlot?.Dispose();
        }
    }
}