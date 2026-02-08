using UnityEngine;
using System.Collections;
using Game.Data;
using Game.Core;
using Game.UI;
using Utils;
using System;
using UnityEngine.Events;

namespace Game.Core
{
    /// <summary>
    /// Manages the core gameplay loop, state, and level progression.
    /// Communicates with AppManager for lifecycle events and GameUIManager for HUD updates.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        public event Action<float> OnTimerUpdate;
        [Header("Dependencies")]
        [SerializeField] private GameDataHolder gameData;
        [SerializeField] private LevelBuilder levelBuilder;
        [SerializeField] private GameUIManager gameUIManager; // Assume this exists as per prompt
        private PoolingManager _poolingManager;

        [Header("Game State")]
        private Coroutine timerCoroutine;

        // Callbacks for AppManager
        public System.Action OnGameCompleted;
        public System.Action OnGameOver;
        public System.Action OnGameExit;

        private float currentTimer;

        public AppManager.AppGameState CurrentAppState => AppManager.CurrentAppState;

        #region Initialization & Lifecycle

        private void OnEnable()
        {
            // Subscribe to UI events if necessary (e.g., Pause button click)
            if (gameUIManager != null)
            {
                gameUIManager.OnQuitClicked += HandleQuitRequest;
            }
            var levelBuilderInstance = LevelBuilder.Instance;
            if (levelBuilderInstance != null)
            {
                levelBuilderInstance.OnAllItemsCollected += HandleVictory;
            }
        }

        private void OnDisable()
        {
            if (gameUIManager != null)
            {
                gameUIManager.OnQuitClicked -= HandleQuitRequest;
            }
        }

        IEnumerator Start()
        {
            _poolingManager = PoolingManager.Instance;
            yield return new WaitForSeconds(.1f);
            // StartGameSession();
        }

        [ContextMenu(nameof(StartGameSession))]
        /// <summary>
        /// Called by AppManager to start a new game session.
        /// </summary>
        public void StartGameSession()
        {
            SetupLevel();
            // Initialize UI
            gameUIManager.InitializeGameUI(levelBuilder.GetSpawnedTargets(), currentTimer);
        }

        #endregion

        #region Level Setup

        private void SetupLevel()
        {
            // Get time limit
            currentTimer = gameData.GetTimeForLevel();
            levelBuilder.BuildLevel();

            // Start Game Loop
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            timerCoroutine = StartCoroutine(GameTimerRoutine());
        }

        #endregion

        #region Gameplay Logic

        private IEnumerator GameTimerRoutine()
        {
            while (currentTimer > 0 && CurrentAppState == AppManager.AppGameState.Playing)
            {
                currentTimer -= Time.deltaTime;
                OnTimerUpdate?.Invoke(currentTimer);
                yield return null;
            }
            // If loop finished because timer ran out
            if (currentTimer <= 0 && CurrentAppState == AppManager.AppGameState.Playing)
            {
                HandleDefeat();
            }
        }

        #endregion

        #region State Management

        internal void HandleResumeRequest()
        {
            StartCoroutine(GameTimerRoutine()); // Resume timer
        }

        private void HandleQuitRequest()
        {
            levelBuilder.ClearExistingLevel();
            OnGameExit?.Invoke();
        }

        private void HandleVictory()
        {
            Debug.Log($"Level completed");
            gameData.IncrementPlayerLevel(); // Increase difficulty for next run
            OnGameCompleted?.Invoke();
        }

        private void HandleDefeat()
        {
            OnGameOver?.Invoke();
        }

        #endregion
    }
}