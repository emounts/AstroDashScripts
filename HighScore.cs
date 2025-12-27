using UnityEngine;
using TMPro;

public class HighScore : MonoBehaviour
{
    public TextMeshProUGUI scoreTextHigh;

    void Update()
    {
        UpdateHighScoreText();
    }

    void UpdateHighScoreText()
    {
        if (scoreTextHigh == null) return;
        scoreTextHigh.text = PlayerPrefs.GetInt(GameConstants.PrefHighScore, 0).ToString();
    }
}
