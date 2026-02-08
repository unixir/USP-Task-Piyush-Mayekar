using UnityEngine;
using System.Collections.Generic;
using System;

namespace Game.Data
{
    /// <summary>
    /// Holds all configuration data for the game levels.
    /// Designed as a ScriptableObject for easy tweaking in the Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "GameDataHolder", menuName = "Game/GameDataHolder")]
    public class GameDataHolder : ScriptableObject
    {
        [Header("Player Data")]
        [SerializeField]
        private int currentPlayerLevel = 1;


        [Header("Level Configuration")]
        [Tooltip("Base number of collectibles to spawn per level.")]
        public int baseCollectibleCount = 10;
        [Tooltip("Additional collectibles added per level.")]
        public int increaseCollectibleCountPerLevel = 1;
        [Tooltip("Base duration for Level 1 in seconds.")]
        public float baseTimeDuration = 180f;

        [Tooltip("Amount of time reduced per level (in seconds).")]
        public float timeDecayPerLevel = 10f;

        [Tooltip("Minimum time allowed for a level.")]
        public float minTimeDuration = 30f;

        [Tooltip("Base size of a grid cell in world units.")]
        public float baseGridCellSize = 1.5f;

        [Tooltip("Minimum size a grid cell can shrink to as difficulty increases.")]
        public float minGridCellSize = 0.8f;

        [Tooltip("How much density increases per level.")]
        public float densityStep = 0.1f;

        [Header("Object Pools")]
        public List<SpawnableSprite> collectibleSprites; // Flowers
        public List<SpawnableSprite> obstacleSprites;    // Grass, Moss, Mushrooms

        [Header("Visual Variation")]
        [Tooltip("Min/Max scale multiplier for spawned objects.")]
        public Vector2 scaleRange = new Vector2(0.8f, 1.2f);

        [Tooltip("Max random rotation in degrees (Z-axis).")]
        public float maxRotationOffset = 15f;

        [Tooltip("Max random position offset from grid center.")]
        public float maxPositionOffset = 0.5f;

        /// <summary>
        /// Calculates the grid cell size based on the current level.
        /// </summary>
        public float GetGridCellSize(int level)
        {
            // As level increases, cell size decreases (higher density)
            float size = baseGridCellSize - (level * densityStep);
            return Mathf.Max(size, minGridCellSize);
        }

        /// <summary>
        /// Calculates the time limit for a specific level.
        /// </summary>
        public float GetTimeForLevel()
        {
            float time = baseTimeDuration - ((CurrentPlayerLevel - 1) * timeDecayPerLevel);
            return Mathf.Max(time, minTimeDuration);
        }

        public int CurrentPlayerLevel { get { return currentPlayerLevel; } private set { currentPlayerLevel = value; } }

        public void IncrementPlayerLevel()
        {
            CurrentPlayerLevel++;
        }

        internal int GetNumberOfCollectiblesToSpawn()
        {
            int baseCount = baseCollectibleCount;
            return baseCount + (CurrentPlayerLevel - 1) * increaseCollectibleCountPerLevel; // Increase by increaseCollectibleCountPerLevel per level
        }

        public void ResetProgress()
        {
            currentPlayerLevel = 1;
        }
    }
}

[System.Serializable]
public class SpawnableSprite
{
    public Sprite sprite;
    [Range(0f, 1f)]
    public float spawnProbability = 0.9f;
    public Vector2 spawnSizeRange = new Vector2(1f, 1f);
}