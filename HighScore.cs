using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// TMPro is needed for the text I'm using
using TMPro;
using System.Globalization;

public class HighScore : MonoBehaviour
{

    public TextMeshProUGUI scoreTextHigh;


    void Update()
    {
        UpdateHighScoreText();
    }

    void UpdateHighScoreText()
    {
        scoreTextHigh.text = $"{PlayerPrefs.GetString("High Score", "1")}";

    }

}

