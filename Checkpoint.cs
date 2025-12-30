using UnityEngine;
using TMPro;
using System.Collections;

public class Checkpoint : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("The laser sprite object to vanish when checkpoint is reached.")]
    [SerializeField] private GameObject checkpointLineVisual;

    private bool isActivated = false;
    public bool IsActivated => isActivated;

    private void OnEnable()
    {
        GameManagerRS gm = FindFirstObjectByType<GameManagerRS>();
        if (gm != null) gm.RegisterCheckpoint(this);
    }

    private void OnDisable()
    {
        GameManagerRS gm = FindFirstObjectByType<GameManagerRS>();
        if (gm != null) gm.UnregisterCheckpoint(this);
    }

    private void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        if (isActivated) return;

        // Check if it's the player
        if (other.GetComponent<RocketMovement1>() != null || other.CompareTag("Player") || other.CompareTag("RocketShip"))
        {
            ActivateCheckpoint(other);
        }
    }

    private void ActivateCheckpoint(GameObject other)
    {
        isActivated = true;

        // 1. Notify GameManager (passing self)
        GameManagerRS gameManager = FindFirstObjectByType<GameManagerRS>();
        if (gameManager != null)
        {
            gameManager.SetCheckpoint(this);
        }

        // 2. Laser sprite vanish
        if (checkpointLineVisual != null)
        {
            checkpointLineVisual.SetActive(false);
        }

        // 3. Text appear via UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowCheckpointMessage("Checkpoint Reached!");
        }
        
        var rocket = other.GetComponentInParent<Rigidbody2D>();
        if (rocket != null)
        {
            GameEvents.RaiseCheckpointReached(rocket.position);
        }
        else
        {
            // fallback if you only have a Transform
            GameEvents.RaiseCheckpointReached(other.transform.position);
        }

    }

    public void ResetVisuals()
    {
        if (checkpointLineVisual != null)
        {
            checkpointLineVisual.SetActive(true);
        }
        // We don't reset isActivated because we don't want to trigger it again immediately if the player spawns on top of it.
        // But if the player moves away and comes back? Usually checkpoints are one-time or re-triggerable.
        // Keeping isActivated = true prevents re-triggering sound/text immediately on respawn.
    }
}
