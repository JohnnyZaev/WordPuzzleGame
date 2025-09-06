using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WPG.Runtime.UI.View
{
    public abstract class View : MonoBehaviour, IView, IDisposable
    {
        public abstract UniTask Show();
        public abstract UniTask Hide();
        public abstract void Dispose();
    }

    public interface IView
    {
        public UniTask Show();
        public UniTask Hide();
    }
}