using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManagerRS : MonoBehaviour
{
    [Header("Game State")]
    [SerializeField] private bool gameHasEnded = false;
    [SerializeField] private float restartDelay = 1f;
    [Tooltip("Vertical offset to apply when respawning at a checkpoint.")]
    [SerializeField] private float respawnHeightOffset = -3f;

    [Header("Object References")]
    [Tooltip("Assign the player's rocket GameObject here.")]
    [SerializeField] private GameObject playerRocket;

    // Static data for checkpoints that persists across player deaths, but not full game restarts.
    private static Checkpoint latestCheckpointScript;
    private static Vector3 latestCheckpointPosition;
    private static bool hasCheckpoint = false;
    
    // Lives System
    [SerializeField] private int startingLives = 3;
    private int currentLives;
    private Vector3 initialStartPosition;
    private GameObject runtimeRocket;
    
    // Level Object Registry (For Progress Bar)
    public HashSet<Checkpoint> AllCheckpoints { get; private set; } = new HashSet<Checkpoint>();
    public HashSet<CelestialObjectController> AllPlanets { get; private set; } = new HashSet<CelestialObjectController>();
    
    [Tooltip("If > 0, this height is used as the 100% mark for the level progress bar. If 0, it auto-calculates based on the highest object.")]
    [SerializeField] private float manualLevelHeight = 0f;

    private void Awake()
    {
        // Reset static checkpoint data on full game initialization/restart
        hasCheckpoint = false;
        latestCheckpointScript = null;
        latestCheckpointPosition = Vector3.zero;

        // Ensure singleton if not already present (simplified for this context)
        // Note: GameManagerRS is often used as a scene-local manager.
    }

    public void RegisterCheckpoint(Checkpoint cp)
    {
        if (cp != null) AllCheckpoints.Add(cp);
    }

    public void UnregisterCheckpoint(Checkpoint cp)
    {
        if (cp != null) AllCheckpoints.Remove(cp);
    }

    public void RegisterPlanet(CelestialObjectController planet)
    {
        if (planet != null) AllPlanets.Add(planet);
    }

    public void UnregisterPlanet(CelestialObjectController planet)
    {
        if (planet != null) AllPlanets.Remove(planet);
    }

    public float GetLevelMaxY()
    {
        if (manualLevelHeight > 0f) return manualLevelHeight;

        float maxY = initialStartPosition.y + 100f; // Default minimum height
        
        foreach (var cp in AllCheckpoints)
        {
            if (cp != null) maxY = Mathf.Max(maxY, cp.transform.position.y);
        }
        foreach (var p in AllPlanets)
        {
            // Planets might spawn high up. Use their appearHeight if set, or current transform Y.
            if (p != null)
            {
                 float y = p.useHeightGate ? p.appearHeight : p.transform.position.y;
                 maxY = Mathf.Max(maxY, y);
            }
        }
        return maxY;
    }

    private void Start()
    {
        currentLives = startingLives;

        // Capture initial start position of the player
        runtimeRocket = ResolveInitialRocket();
        
        // Also ensure SkinManager knows about this rocket if we found one via fallback
        if (runtimeRocket != null && SkinManager.Instance != null && SkinManager.Instance.CurrentRocket == null)
        {
             // If SkinManager doesn't have a rocket but we found one in the scene (legacy), 
             // we might want to tell SkinManager about it, or at least we are good to go locally.
             // SkinManager.Instance.RegisterLegacyRocket(runtimeRocket); // Optional
        }

        if (runtimeRocket != null)
        {
            playerRocket = runtimeRocket;
            initialStartPosition = runtimeRocket.transform.position;
        }
        else if (playerRocket != null)
        {
            initialStartPosition = playerRocket.transform.position;
        }
        else
        {
            GameObject p = GameObject.FindGameObjectWithTag("RocketShip");
            if (p != null) initialStartPosition = p.transform.position;
        }

        // Setup UI
        if (LivesManager.Instance != null)
        {
            Sprite lifeSprite = null;
            if (SkinManager.Instance != null && SkinManager.Instance.CurrentSkin != null)
            {
                lifeSprite = SkinManager.Instance.CurrentSkin.lifeIconSprite;
            }
            LivesManager.Instance.SetupLives(startingLives, lifeSprite);
        }
    }

    private GameObject ResolveInitialRocket()
    {
        // Prefer the skin manager's active rocket if present.
        if (SkinManager.Instance != null)
        {
            GameObject skinRocket = SkinManager.Instance.CurrentRocket;
            if (skinRocket != null) return skinRocket;
        }

        // Fallbacks.
        if (playerRocket != null) return playerRocket;

        GameObject tagged = GameObject.FindGameObjectWithTag("RocketShip");
        if (tagged != null) return tagged;

        RocketMovement1 anyRocket = FindFirstObjectByType<RocketMovement1>();
        return anyRocket != null ? anyRocket.gameObject : null;
    }

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
    public void SetCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;
        
        Vector3 position = checkpoint.transform.position;
        Debug.Log($"GameManagerRS: SetCheckpoint called at {position}");
        hasCheckpoint = true;
        latestCheckpointPosition = position;
        latestCheckpointScript = checkpoint;
    }
    
    // Maintain overload for backward compatibility if needed.
    public void SetCheckpoint(Vector3 position)
    {
        Debug.Log($"GameManagerRS: SetCheckpoint (Vector3) called at {position}");
        hasCheckpoint = true;
        latestCheckpointPosition = position;
    }

    private void OnPlayerCrashed()
    {
        if (gameHasEnded) return;

        Debug.Log($"GameManagerRS: OnPlayerCrashed called. hasCheckpoint: {hasCheckpoint}");
        
        // Handle Lives
        currentLives = Mathf.Max(0, currentLives - 1);
        if (LivesManager.Instance != null)
        {
            LivesManager.Instance.LoseLife();
        }

        if (currentLives <= 0)
        {
            gameHasEnded = true;
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("Game Over", restartDelay);
            }

            // Stop any lingering motion so the rocket isn't flying during the game-over pause.
            GameObject activeRocket = SkinManager.Instance != null ? SkinManager.Instance.CurrentRocket : playerRocket;
            if (activeRocket != null)
            {
                Rigidbody2D rb = activeRocket.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }

            // Check High Score before leaving
            RSScore scoreManager = FindFirstObjectByType<RSScore>();
            if (scoreManager != null) scoreManager.CheckHighScore();

            // Return to Pregame (Scene 0)
            Invoke(nameof(LoadPregame), restartDelay);
        }
        else
        {
            // Respawn (don't end game, just temporary halt)
             Invoke(nameof(RespawnPlayer), restartDelay);
        }
    }

    private void LoadPregame()
    {
        SceneManager.LoadScene(0);
    }

    private void RespawnPlayer()
    {
        Debug.Log("GameManagerRS: RespawnPlayer called.");
        
        // Determine respawn position
        Vector3 respawnPos = hasCheckpoint ? latestCheckpointPosition : initialStartPosition;

        if (hasCheckpoint)
        {
            Debug.Log($"GameManagerRS: Applying Checkpoint Offset: {respawnHeightOffset}");
            respawnPos.y += respawnHeightOffset;
        }
        else
        {
             // Lower the respawn position slightly so the Earth below is visible
             respawnPos.y -= 0f;
        }

        // Reset Checkpoint Visuals if applicable and we are at a checkpoint
        if (hasCheckpoint && latestCheckpointScript != null)
        {
            latestCheckpointScript.ResetVisuals();
        }

        // Try to get the active rocket from SkinManager, otherwise use the assigned fallback.
        GameObject activeRocket = runtimeRocket != null ? runtimeRocket : playerRocket;
        if (activeRocket == null && SkinManager.Instance != null && SkinManager.Instance.CurrentRocket != null)
        {
            activeRocket = SkinManager.Instance.CurrentRocket;
        }

        // Fallback: Find any RocketMovement1 even if inactive
        if (activeRocket == null)
        {
            RocketMovement1[] allRockets = Resources.FindObjectsOfTypeAll<RocketMovement1>();
            foreach (var r in allRockets)
            {
                if (r.gameObject.scene.IsValid()) 
                {
                    activeRocket = r.gameObject;
                    break; 
                }
            }
        }

        if (activeRocket != null)
        {
            Debug.Log($"GameManagerRS: Respawning active rocket: {activeRocket.name} at {respawnPos}");

            // Move and reset orientation/velocity before re-enabling gameplay.
            activeRocket.transform.SetPositionAndRotation(respawnPos, Quaternion.identity);
            Rigidbody2D rb = activeRocket.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.SetRotation(0f);
            }
            activeRocket.SetActive(true);

            // Get the movement script to reset its state.
            RocketMovement1 movementScript = activeRocket.GetComponent<RocketMovement1>();
            if (movementScript != null)
            {
                movementScript.Respawn();
            }

            // Notify systems (like Celestial Objects) that the player has respawned
            GameEvents.RaisePlayerRespawned();
        }
        else
        {
            Debug.LogError("GameManagerRS: No active rocket found to respawn!");
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
