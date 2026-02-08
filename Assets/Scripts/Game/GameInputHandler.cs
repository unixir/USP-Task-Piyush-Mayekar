using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core;
using Game.Data;
using DG.Tweening;
using System.Collections;
using Utils;
using System;
using UnityEngine.UIElements.InputSystem;
using UnityEngine.EventSystems;

namespace Game.Core
{
    /// <summary>
    /// Handles player input using the new Input System.
    /// Performs RaycastAll to detect overlaps and determines interactions (Collect vs Obstacle).
    /// Uses Object Pooling for Particle Systems.
    /// </summary>
    public class GameInputHandler : Singleton<GameInputHandler>
    {
        public event Action<GameObject> OnItemCollected;

        [Header("References")]
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform bagTransform; // The target for collected flowers

        [Header("Input Settings")]
        [SerializeField] private InputActionReference touchAction;
        [SerializeField] private LayerMask interactableLayerMask, nonInteractableLayerMask; // Layer for collectibles and obstacles
        // Cached reference
        private PoolingManager _poolingManager;
        private LevelBuilder _levelBuilder;
        private SoundManager _soundManager;

        // Tag for the particle pool (Must match the Tag in PoolingManager Inspector)
        private const string POOL_TAG_LEAF_PARTICLE = "LeafParticle";
        // Tag for the SFX
        private const string GRASS_TAG = "Grass";
        private const string WHOOSH_TAG = "Whoosh";

        void Start()
        {
            _poolingManager = PoolingManager.Instance;
            _levelBuilder = LevelBuilder.Instance;
            _soundManager = SoundManager.Instance;
        }

        private void OnEnable()
        {
            if (touchAction != null)
            {
                touchAction.action.Enable();
                touchAction.action.performed += HandleInput;
            }
        }

        private void OnDisable()
        {
            if (touchAction != null)
            {
                touchAction.action.performed -= HandleInput;
                touchAction.action.Disable();
            }
        }

        /// <summary>
        /// Main entry point for input.
        /// Updated to work seamlessly in Editor (Mouse) and on Device (Touch).
        /// </summary>
        private void HandleInput(InputAction.CallbackContext context)
        {
            // UI Check: Ignore input if tapping on UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            // Pointer.current is the abstraction for both Mouse and Touch.
            if (Pointer.current == null) return;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            // Safety check to ignore invalid inputs
            if (screenPosition == Vector2.zero) return;

            ProcessTap(screenPosition);
        }

        /// <summary>
        /// Processes the tap position using RaycastAll to handle overlapping objects.
        /// </summary>
        private void ProcessTap(Vector2 screenPos)
        {
            if (gameCamera == null) gameCamera = Camera.main;

            Vector3 worldPos = gameCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;

            RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero, 10f, interactableLayerMask);

            if (hits.Length == 0) return;
            bool isAnyCollectibleHit = false;
            bool isShroomHit = false;
            GameObject firstObjectHit = null;
            SpriteRenderer sr = null;
            foreach (RaycastHit2D hit in hits)
            {
                GameObject hitObject = hit.collider.gameObject;
                sr = hitObject.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                if (_levelBuilder.IsCollectible(hitObject))
                {
                    isAnyCollectibleHit = true;
                    firstObjectHit = hitObject;
                    break; // Prioritize collectibles, exit loop if found
                }
                else if (IsAShroom(sr))
                {
                    isShroomHit = true;
                    firstObjectHit = hitObject;
                    break;
                }
            }
            if (isAnyCollectibleHit)
            {
                HandleCollectibleHit(firstObjectHit);
            }
            else if (isShroomHit)
            {
                AnimateShroom(firstObjectHit, sr);
            }
            else
            {
                SpawnLeafParticle(worldPos);
            }
        }

        #region Interaction Logic

        private void HandleCollectibleHit(GameObject flower)
        {
            if (flower.activeSelf == false) return;

            SpriteRenderer sr = flower.GetComponent<SpriteRenderer>();
            int originalOrder = sr.sortingOrder;
            sr.sortingOrder = _Constants.SORTING_ORDER_INTERACTION;

            Sequence collectSequence = DOTween.Sequence();

            collectSequence.Append(flower.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack));

            if (bagTransform != null)
            {
                collectSequence.Append(flower.transform.DOMove(bagTransform.position, 1f).SetEase(Ease.InOutCirc));
            }
            else
            {
                collectSequence.Append(flower.transform.DOScale(0f, 0.3f));
            }
            collectSequence.OnComplete(() =>
                        {
                            _soundManager.PlaySFX(_Constants.TAG_COLLECTIBLE);
                            OnItemCollected?.Invoke(flower);
                        });
        }

        private static bool IsAShroom(SpriteRenderer sr)
        {
            return sr.sprite != null && sr.sprite.name.ToLower().Contains("shroom");
        }

        private void AnimateShroom(GameObject shroom, SpriteRenderer sr)
        {
            if (DOTween.IsTweening(shroom.transform)) return;

            int originalOrder = sr.sortingOrder;

            shroom.layer = nonInteractableLayerMask;
            Sequence shroomSequence = DOTween.Sequence();
            sr.sortingOrder = _Constants.SORTING_ORDER_INTERACTION;
            float stayDuration = 1f;
            var originalScale = shroom.transform.localScale;
            shroomSequence.Append(shroom.transform.DOScale(originalScale * 1.1f, .5f));
            shroomSequence.AppendInterval(stayDuration);
            _soundManager.PlaySFX(WHOOSH_TAG);
            shroomSequence.Append(shroom.transform.DOScale(originalScale, .5f).SetEase(Ease.OutElastic));
            shroomSequence.AppendInterval(stayDuration);
            shroomSequence.OnComplete(() =>
            {
                sr.sortingOrder = originalOrder;
                shroom.layer = LayerMask.NameToLayer(_Constants.TAG_INTERACTABLE);
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Spawns a particle from the pool and handles its return after playback.
        /// </summary>
        private void SpawnLeafParticle(Vector3 position)
        {
            if (_poolingManager == null) return;

            // Offset slightly in Z so particles render above sprites
            Vector3 spawnPos = position + (Vector3.forward * 1f);

            // Request from Pool
            GameObject particleObj = _poolingManager.SpawnFromPool(POOL_TAG_LEAF_PARTICLE, spawnPos, Quaternion.identity);
            _soundManager.PlaySFX(GRASS_TAG);
            if (particleObj != null)
            {
                ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Play(); // Ensure it plays (Pooled objects might be stopped)
                    float duration = ps.main.duration;

                    // Schedule return to pool
                    StartCoroutine(ReturnParticleAfterTime(particleObj, duration));
                }
            }
            else
            {
                // Fallback or Warning if pool isn't set up
                Debug.LogWarning($"PoolingManager: Could not spawn particle with tag '{POOL_TAG_LEAF_PARTICLE}'. Check Pool setup.");
            }
        }

        private IEnumerator ReturnParticleAfterTime(GameObject particleObj, float delay)
        {
            // Wait for particle to finish playing + a small buffer
            yield return new WaitForSeconds(delay + 0.2f);

            // Stop it cleanly
            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();

            // Return to pool
            if (_poolingManager != null)
            {
                _poolingManager.ReturnToPool(POOL_TAG_LEAF_PARTICLE, particleObj);
            }
        }

        #endregion
    }
}