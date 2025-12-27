using System;

// Lightweight event hub so systems don't hard-reference each other.
public static class GameEvents
{
    public static event Action PlayerCrashed;
    public static event Action<int> ScoreChanged;
    public static event Action<RocketSkin> SkinChanged;
    public static event Action CheckpointReached;

    public static void RaisePlayerCrashed() => PlayerCrashed?.Invoke();
    public static void RaiseScoreChanged(int score) => ScoreChanged?.Invoke(score);
    public static void RaiseSkinChanged(RocketSkin skin) => SkinChanged?.Invoke(skin);
    public static void RaiseCheckpointReached() => CheckpointReached?.Invoke();
}
