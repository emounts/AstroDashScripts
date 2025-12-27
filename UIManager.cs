using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages UI elements for the game scene, such as the checkpoint notification.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    [Tooltip("The TextMeshProUGUI element that displays 'Checkpoint Reached'.")]
    public TextMeshProUGUI checkpointText;

    [Header("Animation Settings")]
    [Tooltip("How long the text stays fully visible before fading.")]
    public float textVisibleDuration = 1f;
    [Tooltip("How long it takes for the text to fade out.")]
    public float textFadeDuration = 0.5f;
    
    private Coroutine _fadeCoroutine;

    private void OnEnable()
    {
        GameEvents.CheckpointReached += ShowCheckpointMessage;
    }

    private void OnDisable()
    {
        GameEvents.CheckpointReached -= ShowCheckpointMessage;
    }

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Ensure the text is invisible on start.
        if (checkpointText != null)
        {
            checkpointText.alpha = 0f;
        }
    }

    /// <summary>
    /// Displays the 'Checkpoint Reached' message and then fades it out.
    /// </summary>
    public void ShowCheckpointMessage()
    {
        if (checkpointText == null) return;
        
        // If a fade is already in progress, stop it to start a new one.
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }
        
        _fadeCoroutine = StartCoroutine(FadeCheckpointText());
    }

    private IEnumerator FadeCheckpointText()
    {
        // Instantly make the text fully visible.
        checkpointText.alpha = 1f;

        // Wait for the specified duration.
        yield return new WaitForSeconds(textVisibleDuration);

        // Fade the text out.
        float elapsedTime = 0f;
        while (elapsedTime < textFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            checkpointText.alpha = 1f - Mathf.Clamp01(elapsedTime / textFadeDuration);
            yield return null;
        }

        // Ensure alpha is exactly 0 at the end.
        checkpointText.alpha = 0f;
    }
}
