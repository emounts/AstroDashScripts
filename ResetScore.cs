using UnityEngine;

public class ResetScore : MonoBehaviour
{
    public void ResetHighScore()
    {
        PlayerPrefs.SetString(GameConstants.PrefHighScore, "0");
    }
}
