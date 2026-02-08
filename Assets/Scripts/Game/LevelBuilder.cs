using UnityEngine;
using Game.Data;
using System.Collections.Generic;
using Utils;
using System;
using Random = UnityEngine.Random;

namespace Game.Core
{
    /// <summary>
    /// Responsible for constructing the game level based on current difficulty.
    /// Handles grid generation, randomization, and object setup.
    /// </summary>
    public class LevelBuilder : Singleton<LevelBuilder>
    {
        public event Action OnAllItemsCollected;
        [Header("References")]
        [SerializeField] private GameDataHolder gameData;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private float spawnAreaPadding = 1f; // Extra padding to ensure coverage beyond screen edges
        private bool areScreenBoundsCalculated = false;
        private Vector2 _screenTopRight;
        private Vector2 _screenBottomLeft;
        private Vector2 screenBounds;

        private HashSet<GameObject> _spawnedCollectibles, _spawnedObstacles;


        private PoolingManager _poolingManager = null;
        private GameInputHandler _inputHandler = null;

        // Define pool tags. These must match the PoolingManager setup.
        private const string POOL_TAG_ITEM = "Interactable";
        private Vector2 ScreenBounds
        {
            get
            {
                if (!areScreenBoundsCalculated) CalculateScreenBounds();
                return _screenTopRight;
            }
        }
        void Start()
        {
            _inputHandler = GameInputHandler.Instance;
            if (_inputHandler != null)
                _inputHandler.OnItemCollected += HandleItemCollected;
        }


        /// <summary>
        /// Builds the level for the specified difficulty level.
        /// Returns the total number of collectibles spawned for UI initialization.
        /// </summary>
        public int BuildLevel()
        {
            if (gameData == null)
            {
                Debug.LogError("GameDataHolder not assigned to LevelBuilder!");
                return 0;
            }

            ClearExistingLevel();
            _spawnedCollectibles = new();
            _spawnedObstacles = new();

            CalculateScreenBounds();

            float cellSize = gameData.GetGridCellSize(gameData.CurrentPlayerLevel);

            // Calculate grid dimensions based on the Padded Width and Height
            float safeWidth = _screenTopRight.x - _screenBottomLeft.x;
            float safeHeight = _screenTopRight.y - _screenBottomLeft.y;
            int columns = Mathf.CeilToInt(safeWidth / cellSize);
            int rows = Mathf.CeilToInt(safeHeight / cellSize);

            List<Vector3> occupiedPositions = new List<Vector3>();

            float startX = _screenBottomLeft.x + (cellSize / 2);
            float startY = _screenBottomLeft.y + (cellSize / 2);
            var _transform = transform; //Caching to avoid extern calls
            var obstacleSprites = gameData.obstacleSprites;
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 gridPos = new Vector3(startX + (x * cellSize), startY + (y * cellSize), 0);

                    // Apply random offset
                    Vector3 randomPos = ApplyRandomOffset(gridPos, gameData.maxPositionOffset);
                    occupiedPositions.Add(randomPos);
                    SpawnableSprite sprite = GetRandomSpawnableSprite(obstacleSprites);

                    GameObject item = GetPoolingManager().SpawnFromPool(POOL_TAG_ITEM, randomPos, Quaternion.identity, _transform);
                    SetupItem(item, sprite, true);
                }
            }

            int flowersToSpawn = gameData.GetNumberOfCollectiblesToSpawn();

            // Shuffle occupiedPositions to randomize flower placement
            ShuffleList(occupiedPositions);

            int spawnedFlowers = 0;
            foreach (Vector3 pos in occupiedPositions)
            {
                if (spawnedFlowers >= flowersToSpawn) break;

                // Pick a flower sprite
                SpawnableSprite flowerSprite = GetRandomSpawnableSprite(gameData.collectibleSprites);

                GameObject flower = GetPoolingManager().SpawnFromPool(POOL_TAG_ITEM, pos, Quaternion.identity, _transform);
                SetupItem(flower, flowerSprite, false);

                spawnedFlowers++;
            }
            Debug.Log($"Total Spawned:{_spawnedCollectibles.Count + _spawnedObstacles.Count} Obstacles: {_spawnedObstacles.Count}, Collectibles: {_spawnedCollectibles.Count}");
            return spawnedFlowers;
        }

        private static SpawnableSprite GetRandomSpawnableSprite(List<SpawnableSprite> spawnableSpritesList)
        {
            if (spawnableSpritesList == null || spawnableSpritesList.Count == 0) return null;
            int i = Random.Range(0, spawnableSpritesList.Count);
            while (true)
            {
                if (Random.Range(0f, 1f) <= spawnableSpritesList[i].spawnProbability)
                {
                    return spawnableSpritesList[i];
                }
                i++;
                i %= spawnableSpritesList.Count;
            }
        }

        /// <summary>
        /// Configures the visual and physical properties of a spawned item.
        /// </summary>
        private void SetupItem(GameObject item, SpawnableSprite spawnable, bool isObstacle)
        {
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError("Pooled Item missing SpriteRenderer!");
                return;
            }

            sr.sprite = spawnable.sprite;

            // Random Scale
            float randomScale = Random.Range(spawnable.spawnSizeRange.x, spawnable.spawnSizeRange.x);
            item.transform.localScale = Vector3.one * randomScale;

            // Random Rotation
            float randomRot = Random.Range(-gameData.maxRotationOffset, gameData.maxRotationOffset);
            item.transform.rotation = Quaternion.Euler(0, 0, randomRot);

            // Sorting Layers & hashset add
            if (isObstacle)
            {
                sr.sortingOrder = Random.Range(_Constants.SORTING_ORDER_OBSTACLES_MIN, _Constants.SORTING_ORDER_OBSTACLES_MAX + 1);
                _spawnedObstacles.Add(item);
            }
            else
            {
                sr.sortingOrder = Random.Range(_Constants.SORTING_ORDER_COLLECTIBLES_MIN, _Constants.SORTING_ORDER_COLLECTIBLES_MAX + 1);
                _spawnedCollectibles.Add(item);
                sr.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f); // Slight color variation for flowers
                // Debug.Log($"Spawned Collectible: {item.name}. Count of collectibles: {_spawnedCollectibles.Count}");
            }

            // Recalculate Collider
            // PolygonCollider2D does not update automatically when sprite changes at runtime.
            // We must destroy the old one and add a new one.
            var oldCollider = item.GetComponent<PolygonCollider2D>();
            if (oldCollider != null) Destroy(oldCollider);

            item.AddComponent<PolygonCollider2D>();
        }

        /// <summary>
        /// Returns the collected item back to the pool and removes it from the active list.
        /// </summary>
        private void HandleItemCollected(GameObject collectible)
        {
            if (_spawnedCollectibles == null || !_spawnedCollectibles.Contains(collectible))
            {
                Debug.LogWarning($"Collected item {collectible.name} was not found in the active collectibles list.");
                return;
            }
            if (_spawnedCollectibles.Count == 0)
            {
                Debug.LogWarning("No collectibles left to collect, but HandleItemCollected was called.");
                return;
            }
            _spawnedCollectibles.Remove(collectible);
            _poolingManager.ReturnToPool(POOL_TAG_ITEM, collectible);
            Debug.Log($"Collected: {collectible.name}. Remaining collectibles: {_spawnedCollectibles.Count}");
            if (_spawnedCollectibles.Count == 0)
            {
                OnAllItemsCollected?.Invoke();
            }
        }

        /// <summary>
        /// Clears the current level by returning all active items to the pool.
        /// For the builder reset, we find them by tag.
        /// </summary>
        public void ClearExistingLevel()
        {
            try
            {
                ClearHashSetGameObjects(_spawnedCollectibles);
                ClearHashSetGameObjects(_spawnedObstacles);
            }
            catch (Exception e)
            {
                Debug.Log($"Error {e} in {nameof(ClearExistingLevel)}");
            }

            void ClearHashSetGameObjects(HashSet<GameObject> _hashSet)
            {
                if (_hashSet == null) return;
                if (_hashSet != null)
                    foreach (GameObject item in _hashSet)
                    {
                        PoolingManager.Instance.ReturnToPool(POOL_TAG_ITEM, item);
                    }
                _hashSet.Clear();
            }
        }

        private Vector3 ApplyRandomOffset(Vector3 original, float maxOffset)
        {
            var finalOffset = Mathf.Min(maxOffset, spawnAreaPadding); // Ensure offset does not exceed padding
            var offset = new Vector3(
                Random.Range(-finalOffset, finalOffset),
                Random.Range(-finalOffset, finalOffset),
                0);
            return original + offset;
        }

        private void CalculateScreenBounds()
        {
            if (gameCamera == null) gameCamera = Camera.main;

            // Get the raw world coordinates of the screen corners
            Vector3 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, gameCamera.nearClipPlane));
            Vector3 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1, 1, gameCamera.nearClipPlane));

            // Apply padding to create a "Safe Area"
            _screenTopRight = new Vector2(topRight.x - spawnAreaPadding, topRight.y - spawnAreaPadding);
            _screenBottomLeft = new Vector2(bottomLeft.x + spawnAreaPadding, bottomLeft.y + spawnAreaPadding);

            areScreenBoundsCalculated = true;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private PoolingManager GetPoolingManager()
        {
            if (_poolingManager == null)
                _poolingManager = PoolingManager.Instance;
            return _poolingManager;
        }

        public bool IsCollectible(GameObject obj)
        {
            return _spawnedCollectibles != null && _spawnedCollectibles.Contains(obj);
        }

        internal HashSet<GameObject> GetSpawnedTargets()
        {
            return _spawnedCollectibles;
        }
    }
}