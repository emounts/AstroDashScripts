using UnityEngine;
using TMPro;
using System.Globalization;
using System;

public class RSScore : MonoBehaviour
{
    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    public TextMeshProUGUI scoreText;

    private float _startY = float.NaN;
    private int _lastScoreInt = -1;

    // These static variables hold the score and are not reset on scene load by default.
    public static float score;

    void Awake()
    {
        // Reset the score at the beginning of the game.
        score = 0;
        _startY = float.NaN;
        _lastScoreInt = -1;
        if (scoreText != null)
        {
            scoreText.text = "0";
        }
    }

    void Update()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;

        Transform current = skinManager != null ? skinManager.CurrentRocketTransform : null;
        if (current != null)
        {
            UpdateScoreFromY(current.position.y);
        }
    }

    private void UpdateScoreFromY(float y)
    {
        // On the first frame, capture the starting Y position.
        if (float.IsNaN(_startY))
        {
            _startY = y;
        }

        // Score is based on the distance traveled from the start, never negative.
        float distance = y - _startY;
        score = Math.Max(0, distance * 1000f);

        int scoreInt = (int)Math.Round(score);
        if (scoreText != null)
            scoreText.text = scoreInt.ToString("0", CultureInfo.InvariantCulture);

        if (scoreInt != _lastScoreInt)
        {
            _lastScoreInt = scoreInt;
            GameEvents.RaiseScoreChanged(scoreInt);
        }
    }

    public void CheckHighScore()
    {
        int scoreInt = (int)Math.Round(score);
        if (scoreInt > PlayerPrefs.GetInt(GameConstants.PrefHighScore, 0))
        {
            PlayerPrefs.SetInt(GameConstants.PrefHighScore, scoreInt);
        }
    }
}