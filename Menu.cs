using UnityEngine;
// We want it to load scene:
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    public void StartGame()
    {
        // Block starting while any skin carousel is visible/active.
        // if (SkinCarousel.AnyOpen())
        //    return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);


    }
}
