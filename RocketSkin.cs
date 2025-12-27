using UnityEngine;

public enum UnlockType
{
    Free = 0,
    ScoreThreshold = 1,
}

public enum LockOverride
{
    None = 0,
    ForceUnlocked = 1,
    ForceLocked = 2,
}

[System.Serializable]
public struct UnlockRequirement
{
    public UnlockType type;
    public int requiredScore;

    public bool IsUnlocked(int bestScore)
    {
        switch (type)
        {
            case UnlockType.Free:
                return true;
            case UnlockType.ScoreThreshold:
                return bestScore >= requiredScore;
            default:
                return true;
        }
    }
}

[System.Serializable]
public struct RocketMovementTuning
{
    [Header("Horizontal + Vertical Movement")]
    public float initialSpeed;
    public float finalSpeed;
    public float maxHeightSpeedIncrease;
    public float verticalSpeedMultiplier;

    [Header("Input")]
    public bool useMouseInput;
    public KeyCode keyboardKey;
}

// Data-driven definition of a rocket skin + its gameplay tuning.
// Add skins via Create -> Rocket -> RocketSkin.
[CreateAssetMenu(menuName = "Rocket/RocketSkin", fileName = "RocketSkin")]
public class RocketSkin : ScriptableObject
{
    [Header("Identity")]
    public string skinId = "skin_default";
    public string displayName = "Default";

    [Header("Content")]
    public GameObject rocketPrefab;
    public Sprite uiSprite;

    [Header("Unlocking")]
    public UnlockRequirement unlockRequirement;
    [Tooltip("Optional per-skin override for locking/unlocking without changing score logic.")]
    public LockOverride lockOverride = LockOverride.None;

    [Header("Gameplay")]
    public RocketMovementTuning movement;
}
