using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;
using TMPro;
using Cysharp.Threading.Tasks;
using WPG.Runtime.Gameplay.ViewModels;
using WPG.Runtime.UI.View;

namespace WPG.Runtime.Gameplay.Views
{
    public class VictoryView : View
    {
        [Header("UI References")]
        [SerializeField] private Transform _wordsContainer;
        [SerializeField] private TMP_Text _wordTextPrefab;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private TMP_Text _congratsText;
        [SerializeField] private TMP_Text _levelCompletedText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _wordTextColor = Color.white;
        [SerializeField] private float _wordSpacing = 30f;
        
        private readonly CompositeDisposable _disposables = new();
        private readonly List<TMP_Text> _wordTextInstances = new();
        private VictoryViewModel _viewModel;
        
        public void Initialize(VictoryViewModel viewModel)
        {
            _viewModel = viewModel;
            
            _viewModel.CompletedWords
                .Subscribe(UpdateWordsDisplay)
                .AddTo(_disposables);
                
            _viewModel.CurrentLevel
                .Subscribe(UpdateLevelText)
                .AddTo(_disposables);
                
            _viewModel.HasNextLevel
                .Subscribe(UpdateNextLevelButton)
                .AddTo(_disposables);
            
            if (_mainMenuButton != null)
            {
                _mainMenuButton.OnClickAsObservable()
                    .ThrottleFirst(TimeSpan.FromSeconds(1))
                    .Subscribe(_ => _viewModel.MainMenuCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
            }
            
            if (_nextLevelButton != null)
            {
                _nextLevelButton.OnClickAsObservable()
                    .ThrottleFirst(TimeSpan.FromSeconds(1))
                    .Subscribe(_ => _viewModel.NextLevelCommand.Execute(Unit.Default))
                    .AddTo(_disposables);
            }
            
            if (_congratsText != null)
            {
                _congratsText.text = "Поздравляем!";
            }
        }
        
        private void UpdateWordsDisplay(List<string> completedWords)
        {
            foreach (var wordText in _wordTextInstances)
            {
                if (wordText != null)
                {
                    DestroyImmediate(wordText.gameObject);
                }
            }
            _wordTextInstances.Clear();
            
            if (completedWords != null && _wordTextPrefab != null && _wordsContainer != null)
            {
                for (int i = 0; i < completedWords.Count; i++)
                {
                    var wordText = Instantiate(_wordTextPrefab, _wordsContainer, false);
                    wordText.text = completedWords[i];
                    wordText.color = _wordTextColor;
                    
                    var rectTransform = wordText.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(0, -i * _wordSpacing);
                    }
                    
                    _wordTextInstances.Add(wordText);
                }
            }
        }
        
        private void UpdateLevelText(int currentLevel)
        {
            if (_levelCompletedText != null)
            {
                _levelCompletedText.text = $"Уровень {currentLevel} завершён!";
            }
        }
        
        private void UpdateNextLevelButton(bool hasNextLevel)
        {
            if (_nextLevelButton != null)
            {
                _nextLevelButton.interactable = hasNextLevel;
                _nextLevelButton.gameObject.SetActive(hasNextLevel);
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
            
            foreach (var wordText in _wordTextInstances)
            {
                if (wordText != null)
                {
                    DestroyImmediate(wordText.gameObject);
                }
            }
            _wordTextInstances.Clear();
        }
    }
}