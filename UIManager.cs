using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("In-Game Messages")]
    [Tooltip("The TextMeshProUGUI component used for general in-game text.")]
    [SerializeField] private TextMeshProUGUI generalMessageText;
    
    [Tooltip("The TextMeshProUGUI component specifically for checkpoint messages.")]
    [SerializeField] private TextMeshProUGUI checkpointMessageText;
    
    [SerializeField] private float defaultFadeDuration = 1f;

    private Coroutine currentFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (generalMessageText != null)
        {
            generalMessageText.alpha = 0f;
            generalMessageText.gameObject.SetActive(false);
        }
        if (checkpointMessageText != null)
        {
            checkpointMessageText.alpha = 0f;
            checkpointMessageText.gameObject.SetActive(false);
        }
    }

    public void ShowCheckpointMessage(string text, float duration = 2f)
    {
        // Use checkpoint-specific text if assigned, otherwise fallback to general
        TextMeshProUGUI targetText = checkpointMessageText != null ? checkpointMessageText : generalMessageText;
        ShowMessageOnText(targetText, text, duration);
    }

    public void ShowMessage(string text, float duration)
    {
        ShowMessageOnText(generalMessageText, text, duration);
    }

    private void ShowMessageOnText(TextMeshProUGUI textObj, string text, float duration)
    {
        if (textObj == null) return;

        textObj.text = text;
        textObj.gameObject.SetActive(true);
        textObj.alpha = 1f;

        // Note: This simple implementation stops any *global* fade routine. 
        // If we want multiple simultaneous texts, we'd need per-text coroutines. 
        // For now, assuming single-channel UI focus is fine or user assigns different objects.
        if (currentFadeRoutine != null)
        {
            StopCoroutine(currentFadeRoutine);
        }
        currentFadeRoutine = StartCoroutine(FadeTextRoutine(textObj, duration));
    }

    private IEnumerator FadeTextRoutine(TextMeshProUGUI textObj, float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);

        float elapsed = 0f;
        float startAlpha = textObj.alpha;

        while (elapsed < defaultFadeDuration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsed / defaultFadeDuration);
            textObj.alpha = newAlpha;
            yield return null;
        }

        textObj.alpha = 0f;
        textObj.gameObject.SetActive(false);
        currentFadeRoutine = null;
    }
}
