using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerRS : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool gameHasEnded = false;
    [SerializeField] private float restartDelay = 1f;

    [Header("Object References")]
    [Tooltip("Assign the player's rocket GameObject here.")]
    [SerializeField] private GameObject playerRocket;

    // Static data for checkpoints that persists across player deaths, but not full game restarts.
    private static Vector3 latestCheckpointPosition;
    private static bool hasCheckpoint = false;

    private void OnEnable()
    {
        GameEvents.PlayerCrashed += OnPlayerCrashed;
    }

    private void OnDisable()
    {
        GameEvents.PlayerCrashed -= OnPlayerCrashed;
    }

    /// <summary>
    /// Called by a Checkpoint object when the player passes through it.
    /// </summary>
    public void SetCheckpoint(Vector3 position)
    {
        hasCheckpoint = true;
        latestCheckpointPosition = position;
    }

    private void OnPlayerCrashed()
    {
        if (gameHasEnded) return;

        gameHasEnded = true;

        // Save the high score
        RSScore scoreManager = FindObjectOfType<RSScore>();
        if (scoreManager != null)
        {
            scoreManager.CheckHighScore();
        }

        // Decide whether to respawn at a checkpoint or restart the level.
        if (hasCheckpoint)
        {
            Invoke(nameof(RespawnPlayer), restartDelay);
        }
        else
        {
            Invoke(nameof(RestartLevel), restartDelay);
        }
    }

    private void RespawnPlayer()
    {
        if (playerRocket != null)
        {
            // Move the rocket to the checkpoint and reset its state.
            playerRocket.transform.position = latestCheckpointPosition;
            playerRocket.SetActive(true);

            // Get the movement script to reset its state.
            RocketMovement1 movementScript = playerRocket.GetComponent<RocketMovement1>();
            if (movementScript != null)
            {
                movementScript.Respawn();
            }
        }
        
        // The game is no longer "ended" and is ready to be played again.
        gameHasEnded = false;
    }

    private void RestartLevel()
    {
        // When restarting the level, also reset the checkpoint progress.
        hasCheckpoint = false;
        int targetIndex = Mathf.Max(0, SceneManager.GetActiveScene().buildIndex - 1);
        SceneManager.LoadScene(targetIndex);
    }
}
