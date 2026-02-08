using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using DG.Tweening;
using NUnit.Framework;
using Utils;
using Game.Core;
using TMPro;
using UI;

namespace Game.UI
{
    /// <summary>
    /// Handles all in-game UI logic including the HUD, Timer, Target List, and Pause Menu.
    /// Dispatches events to the GameManager for button interactions.
    /// </summary>
    public class GameUIManager : BaseUIScreen
    {
        [Header("HUD Elements")]
        [SerializeField] private Slider timerSlider;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Image timerFillImage;
        [Tooltip("Parent for flower icons")]
        [SerializeField] private Transform targetListContainer;
        [SerializeField] private GameObject targetIconPrefab; // The UI icon for a single flower

        [Header("Buttons")]
        [SerializeField] private Button pauseButton;

        [Header("Pause Menu Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private PoolingManager _poolingManager;
        // State
        private Dictionary<GameObject, GameObject> targetIconMap = new Dictionary<GameObject, GameObject>(); // Maps target GameObjects to their icons
        private float maxTimerDuration;

        // Events to be listened to by GameManager
        public event Action OnPauseClicked;
        public event Action OnResumeClicked;
        public event Action OnRestartClicked;
        public event Action OnQuitClicked;

        #region Unity Lifecycle

        private void Awake()
        {
            // Setup button listeners
            if (pauseButton != null) pauseButton.onClick.AddListener(() => OnPauseClicked?.Invoke());
            if (resumeButton != null) resumeButton.onClick.AddListener(() =>
            {
                SoundManager.Instance?.PlayMusic("GameMusic");
                OnResumeClicked?.Invoke();
            });
            if (restartButton != null) restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnQuitClicked?.Invoke());

            // Enable HUD buttons
            if (pauseButton != null) pauseButton.interactable = true;
        }

        void Start()
        {
            _poolingManager = PoolingManager.Instance;
            var inputHandler = GameInputHandler.Instance;
            if (inputHandler != null)
                inputHandler.OnItemCollected += OnCollectibleCollected;
            var gameManager = GameManager.Instance;
            if (gameManager != null)
                gameManager.OnTimerUpdate += UpdateTimer;
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
            if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
            if (restartButton != null) restartButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
            var gameManager = GameManager.Instance;
            if (gameManager != null)
                gameManager.OnTimerUpdate -= UpdateTimer;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the UI for a new level.
        /// </summary>
        /// <param name="totalTargets">Total number of flowers to find.</param>
        /// <param name="duration">Total time for the level in seconds.</param>
        public void InitializeGameUI(HashSet<GameObject> totalTargets, float duration)
        {
            maxTimerDuration = duration;

            // Setup Timer
            if (timerSlider != null)
            {
                timerSlider.maxValue = duration;
                timerSlider.value = duration;
            }
            else
            {
                Debug.LogWarning("GameUIManager: Timer Slider is not assigned.");
            }
            SoundManager.Instance?.PlayMusic("GameMusic");
            // Setup Target List
            PopulateTargetList(totalTargets);
        }

        private void PopulateTargetList(HashSet<GameObject> totalTargets)
        {
            // Clear existing icons
            foreach (var icon in targetIconMap.Values)
            {
                if (icon != null) _poolingManager.ReturnToPool(_Constants.TAG_COLLECTIBLE_ICON, icon);
            }
            targetIconMap.Clear();

            if (targetListContainer == null)
            {
                Debug.LogWarning("GameUIManager: Target List Container is not assigned.");
                return;
            }

            if (targetIconPrefab == null)
            {
                Debug.LogWarning("GameUIManager: Target Icon Prefab is not assigned. Cannot build list.");
                return;
            }
            Debug.Log($"Populating Target List with {totalTargets.Count} targets.");
            int i = 0;
            // Instantiate new icons
            foreach (var targetCollectible in totalTargets)
            {
                GameObject iconObj = _poolingManager.SpawnFromPool(_Constants.TAG_COLLECTIBLE_ICON, Vector3.zero,
                Quaternion.identity, targetListContainer);
                var spriteIcon = iconObj.GetComponent<Image>();
                var targetSR = targetCollectible.GetComponent<SpriteRenderer>();
                spriteIcon.sprite = targetSR.sprite;
                spriteIcon.color = targetSR.color;
                targetCollectible.SetActive(true);
                targetIconMap[targetCollectible] = iconObj;

                // Pop-in animation using DOTween
                iconObj.transform.localScale = Vector3.zero;
                iconObj.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetDelay(i * 0.05f);
                i++;
            }
        }

        #endregion

        #region HUD Updates

        /// <summary>
        /// Updates the timer slider visual.
        /// </summary>
        /// <param name="currentTime">Current remaining time.</param>
        public void UpdateTimer(float currentTime)
        {
            if (timerSlider != null)
            {
                timerSlider.value = currentTime;
            }

            if (timerFillImage != null)
            {
                float pct = currentTime / maxTimerDuration;
                if (pct < 0.2f)
                {
                    timerFillImage.color = Color.red;
                }
                else
                {
                    timerFillImage.color = Color.white;
                }
            }
            if (timerText != null)
            {
                timerText.text = $"{Mathf.FloorToInt(currentTime / 60):0}:{Mathf.FloorToInt(currentTime % 60):00}";
            }
        }

        /// <summary>
        /// Updates the target list visuals based on remaining items.
        /// </summary>
        /// <param name="remainingCount">How many items are left to find.</param>
        public void OnCollectibleCollected(GameObject collectiblePicked)
        {
            if (targetIconMap.ContainsKey(collectiblePicked))
            {
                GameObject iconObj = targetIconMap[collectiblePicked];
                iconObj.transform.DOScale(0f, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
                {
                    _poolingManager.ReturnToPool(_Constants.TAG_COLLECTIBLE_ICON, iconObj);
                });
                targetIconMap.Remove(collectiblePicked);
            }
            else
            {
                Debug.LogWarning($"Updated Target List: Attempted to update non-existent icon for {collectiblePicked.name}");
            }
        }

        #endregion
    }
}