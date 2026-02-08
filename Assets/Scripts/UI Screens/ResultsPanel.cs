using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UI;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Results Screen overlay. Displays Level Complete or Game Over based on the passed message.
    /// </summary>
    public class ResultsPanel : BaseUIScreen
    {
        public event Action OnNextLevelClicked;
        public event Action OnRetryClicked;
        public event Action OnMenuClicked;

        [Header("UI Elements")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;

        [Header("Visuals")]
        [SerializeField] private Color winColor = Color.green;
        [SerializeField] private Color loseColor = Color.red;

        private Sequence _animSequence;

        private void Awake()
        {
            // Setup Listeners
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(() => OnNextLevelClicked?.Invoke());
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (menuButton != null) menuButton.onClick.AddListener(() => OnMenuClicked?.Invoke());
        }

        public override void Activate(string message = null)
        {
            base.Activate(message);

            // Determine result based on message
            bool isWin = message == _Constants.MSG_RESULT_WIN;
            var soundManager = SoundManager.Instance;
            if (soundManager != null)
            {
                soundManager.StopMusic();
                soundManager.PlaySFX(isWin ? "Victory" : "Defeat");
            }
            ConfigureUI(isWin);
        }


        public override void OnBackButtonPressed()
        {
            // On results screen, Back button acts as "Go to Menu"
            OnMenuClicked?.Invoke();
        }

        private void ConfigureUI(bool isWin)
        {
            if (titleText != null)
            {
                titleText.text = isWin ? _Constants.TEXT_LEVEL_COMPLETE : _Constants.TEXT_GAME_OVER;
                titleText.color = isWin ? winColor : loseColor;
            }

            if (messageText != null)
            {
                messageText.text = isWin
                    ? _Constants.TEXT_WIN_MESSAGE
                    : _Constants.TEXT_LOSE_MESSAGE;
            }

            // Only show Next Level button if the player won
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(isWin);
            }
            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(!isWin);
            }
        }

    }
}