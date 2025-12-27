using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManagerRS gameManager = FindObjectOfType<GameManagerRS>();
            if (gameManager != null)
            {
                gameManager.SetCheckpoint(transform.position);
            }

            GameEvents.RaiseCheckpointReached();
            gameObject.SetActive(false);
        }
    }
}