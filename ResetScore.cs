using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetScore : MonoBehaviour
{
    public void ResetHighScore()
    {
        PlayerPrefs.SetString("High Score","0");
    }
}
