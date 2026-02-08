using UnityEngine;

/// <summary>
/// Centralized constant values for the application to avoid magic numbers.
/// </summary>
public static class _Constants
{
    // --- Game Specific Constants ---

    /// <summary>
    /// Sorting Layer Names (Must match Project Settings > Tags and Layers)
    /// </summary>
    public const string SORTING_LAYER_FOREGROUND = "Foreground";
    public const string SORTING_LAYER_DEFAULT = "Default";

    /// <summary>
    /// Obstacles (Grass/Moss) are 20-30 to cover Collectibles.
    /// Collectibles (Flowers/Shrooms) are 10-20.
    /// </summary>
    public const int SORTING_ORDER_OBSTACLES_MIN = 20;
    public const int SORTING_ORDER_OBSTACLES_MAX = 30;

    public const int SORTING_ORDER_COLLECTIBLES_MIN = 10;
    public const int SORTING_ORDER_COLLECTIBLES_MAX = 20;

    /// <summary>
    /// The temporary sorting order used when an item is interacted with 
    /// to ensure it pops above everything else.
    /// </summary>
    public const int SORTING_ORDER_INTERACTION = 100;

    /// <summary>
    /// Tag for identifying interactable items via Raycasts.
    /// </summary>
    public const string TAG_INTERACTABLE = "Interactable";
    public const string TAG_COLLECTIBLE_ICON = "CollectibleIcon";
    public const string TAG_COLLECTIBLE = "Collectible";

    // --- Result Messages ---
    public const string MSG_RESULT_WIN = "Win";
    public const string MSG_RESULT_LOSE = "Lose";

    // --- UI Text ---
    public const string TEXT_LEVEL_COMPLETE = "Level Complete!";
    public const string TEXT_GAME_OVER = "Game Over";
    public const string TEXT_WIN_MESSAGE = "Great job finding all the flowers!";
    public const string TEXT_LOSE_MESSAGE = "Time ran out! Try again?";
}