using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UI;
using Game.Core;

namespace Game.UI
{
    /// <summary>
    /// Pause Screen overlay. Handles Resume, Restart, and Quit interactions.
    /// Note: Inherits from BaseUIScreen, not Singleton, as it is managed by ScreenManager.
    /// </summary>
    public class PauseScreen : BaseUIScreen
    {
        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        [Header("UI Elements")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private Sequence _animSequence;

        void Awake()
        {
            // Setup Listeners
            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());
        }

        public override void Activate(string message = null)
        {
            base.Activate(message);
            SoundManager.Instance?.StopMusic();
        }

        public override void OnBackButtonPressed()
        {
            OnResumeClicked?.Invoke();
        }

    }
}