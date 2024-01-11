using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerRS : MonoBehaviour
{
    bool gameHasEnded = false;
    public float restartDelay = 1f;


    public void EndGame()
    {
        if (gameHasEnded == false)
        {
            gameHasEnded = true;

            // Invoke adds a delay based off of the second argument
            Invoke("Restart", restartDelay);


        }
    }

    void Restart()
    {
        // Function loads the scene with the current name
        // Argument returns the name of the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
}
