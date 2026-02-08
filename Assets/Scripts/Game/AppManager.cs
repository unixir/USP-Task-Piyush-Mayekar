using UnityEngine;
using UnityEngine.InputSystem;
using UI;
using Game.Core;
using Game.Data;
using System;
using Game.UI;

namespace Game.Core
{
    /// <summary>
    /// Central manager for App Lifecycle, Navigation, and Input handling.
    /// Inherits from Singleton to ensure global access.
    /// </summary>
    public class AppManager : Singleton<AppManager>
    {
        [Header("References")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private GameDataHolder gameData;
        private ScreenManager _screenManager;
        // State tracking
        private static AppGameState _currentState = AppGameState.MainMenu;

        public static AppGameState CurrentAppState => _currentState;

        [Serializable]
        public enum AppGameState
        {
            MainMenu,
            Playing,
            Paused,
            Results
        }

        // We use Awake instead of Start for DontDestroyOnLoad to ensure it's set immediately
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            InitializeSubscriptions();
            LoadMainMenu();
        }

        private void OnDestroy()
        {
            CleanupSubscriptions();
        }

        #region Initialization

        private void InitializeSubscriptions()
        {
            _screenManager = ScreenManager.Instance;
            // Subscribe to Main Menu Events
            var mainMenu = (MainMenuScreen)_screenManager.GetScreen<MainMenuScreen>();
            if (mainMenu != null)
            {
                mainMenu.OnStartGameClicked += StartGame;
                mainMenu.OnExitGameClicked += ExitApplication;
            }

            // Subscribe to Pause Screen Events
            var pauseScreen = (PauseScreen)_screenManager.GetScreen<PauseScreen>();
            if (pauseScreen != null)
            {
                pauseScreen.OnResumeClicked += ResumeGame;
                pauseScreen.OnRestartClicked += RestartLevel;
                pauseScreen.OnQuitClicked += QuitToMainMenu;
            }

            // Subscribe to Game Manager Events
            if (_gameManager != null)
            {
                _gameManager.OnGameCompleted += HandleGameCompleted;
                _gameManager.OnGameOver += HandleGameOver;
                _gameManager.OnGameExit += QuitToMainMenu;
            }

            var resultsPanel = (ResultsPanel)_screenManager.GetScreen<ResultsPanel>();
            if (resultsPanel != null)
            {
                resultsPanel.OnNextLevelClicked += HandleNextLevel;
                resultsPanel.OnRetryClicked += RestartLevel;
                resultsPanel.OnMenuClicked += QuitToMainMenu;
            }


            var gameUIManager = (GameUIManager)_screenManager.GetScreen<GameUIManager>();
            if (gameUIManager != null)
            {
                gameUIManager.OnPauseClicked += ShowPauseScreen;
                gameUIManager.OnResumeClicked += ResumeGame;
                gameUIManager.OnRestartClicked += RestartLevel;
                gameUIManager.OnQuitClicked += QuitToMainMenu;
            }
        }

        private void CleanupSubscriptions()
        {

            var mainMenu = (MainMenuScreen)_screenManager.GetScreen<MainMenuScreen>();
            if (mainMenu != null)
            {
                mainMenu.OnStartGameClicked -= StartGame;
                mainMenu.OnExitGameClicked -= ExitApplication;
            }

            var pauseScreen = (PauseScreen)_screenManager.GetScreen<PauseScreen>();
            if (pauseScreen != null)
            {
                pauseScreen.OnResumeClicked -= ResumeGame;
                pauseScreen.OnRestartClicked -= RestartLevel;
                pauseScreen.OnQuitClicked -= QuitToMainMenu;
            }

            if (_gameManager != null)
            {
                _gameManager.OnGameCompleted -= HandleGameCompleted;
                _gameManager.OnGameOver -= HandleGameOver;
                _gameManager.OnGameExit -= QuitToMainMenu;
            }

            var gameUIManager = (GameUIManager)_screenManager.GetScreen<GameUIManager>();
            if (gameUIManager != null)
            {
                gameUIManager.OnPauseClicked -= ShowPauseScreen;
                gameUIManager.OnResumeClicked -= ResumeGame;
                gameUIManager.OnRestartClicked -= RestartLevel;
                gameUIManager.OnQuitClicked -= QuitToMainMenu;
            }
        }

        #endregion

        #region Navigation & State

        private void LoadMainMenu()
        {
            _currentState = AppGameState.MainMenu;
            _screenManager.ActivateScreen<MainMenuScreen>();
        }

        private void StartGame()
        {
            _currentState = AppGameState.Playing;
            _screenManager.ActivateScreen<GameUIManager>();
            _gameManager.StartGameSession();
        }

        public void ShowPauseScreen()
        {
            if (_currentState != AppGameState.Playing) return;

            _currentState = AppGameState.Paused;
            _screenManager.ActivateScreen<PauseScreen>();
        }

        public void ResumeGame()
        {
            if (_currentState != AppGameState.Paused) return;
            _currentState = AppGameState.Playing;
            _screenManager.ActivateScreen<GameUIManager>();
            _gameManager.HandleResumeRequest();
        }

        private void RestartLevel()
        {
            _currentState = AppGameState.Playing;
            _screenManager.ActivateScreen<GameUIManager>();

            if (_gameManager != null) _gameManager.StartGameSession();
        }

        private void QuitToMainMenu()
        {
            _screenManager.DeactivateScreen<PauseScreen>();
            LoadMainMenu();
        }

        private void HandleGameCompleted()
        {
            _currentState = AppGameState.Results;
            // Pass "Win" message to the screen
            ScreenManager.Instance.ActivateScreen<ResultsPanel>(_Constants.MSG_RESULT_WIN);
        }

        private void HandleGameOver()
        {
            _currentState = AppGameState.Results;
            // Pass "Lose" message to the screen
            ScreenManager.Instance.ActivateScreen<ResultsPanel>(_Constants.MSG_RESULT_LOSE);
        }

        // New handlers for Results Buttons
        private void HandleNextLevel()
        {
            StartGame();
        }

        private void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}