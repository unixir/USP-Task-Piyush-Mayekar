using System;
using DG.Tweening;
using Game.Core;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Main Menu Screen logic. Currently minimal but can be expanded for animations, button interactions, etc.
    /// </summary>
    public class MainMenuScreen : BaseUIScreen
    {
        public event Action OnStartGameClicked;
        public event Action OnExitGameClicked;
        [SerializeField] private Data.GameDataHolder gameData;
        [SerializeField] private Button startGameButton, exitGameButton, resetButton;
        [SerializeField] private TMP_Text playerLevelText, titleText;
        Sequence animSequence;

        void Start()
        {
            startGameButton.onClick.AddListener(() =>
            {
                OnStartGameClicked?.Invoke();
            });
            exitGameButton.onClick.AddListener(() =>
            {
                OnExitGameClicked?.Invoke();
            });
            resetButton.onClick.AddListener(() =>
            {
                gameData.ResetProgress();
                UpdatePlayerLevelText();
            });
        }

        public override void Activate(string message = null)
        {
            base.Activate(message);
            UpdatePlayerLevelText();
            animSequence = DOTween.Sequence();
            animSequence.Append(titleText.transform.DOScale(1.2f, 0.5f).From(1f).SetEase(Ease.OutBack))
                        .Append(titleText.transform.DOScale(1f, 0.5f).SetEase(Ease.InBack));
            animSequence.Append(startGameButton.transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutBack));
            animSequence.SetLoops(-1, LoopType.Yoyo);
            animSequence.Play();
            SoundManager.Instance?.PlayMusic("MainMenuMusic");
        }

        private void UpdatePlayerLevelText()
        {
            playerLevelText.text = $"Player Level: {gameData.CurrentPlayerLevel}";
        }

        public override void Deactivate()
        {
            base.Deactivate();
            animSequence.Kill();
            startGameButton.transform.localScale = Vector3.one;
            titleText.transform.localScale = Vector3.one;
        }
    }
}