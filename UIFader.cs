using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class UIFader : MonoBehaviour
{
    [Header("Fader Settings")]
    [Tooltip("The UI GameObjects to fade. The script will automatically add CanvasGroups if missing.")]
    public List<GameObject> uiElements = new List<GameObject>();

    [Tooltip("Time in seconds for the fade to complete.")]
    public float fadeDuration = 1.0f;

    [Tooltip("If true, these elements will fade in automatically when the game starts (On Start).")]
    public bool fadeInOnStart = false;
    
    [Tooltip("If true, these elements will fade out automatically when the game starts (On Start).")]
    public bool fadeOutOnStart = false;

    [Tooltip("If true, directly fades the alpha of any Image/Text components in children. Useful if CanvasGroup is not working.")]
    public bool fadeChildGraphics = false;

    [Header("Events")]
    public UnityEvent onFadeComplete;

    // Cache the actual CanvasGroups to avoid GetComponent calls during the fade loop
    private List<CanvasGroup> _canvasGroups = new List<CanvasGroup>();
    private bool isWaitingToFade = false;

    private void Awake()
    {
        InitializeCanvasGroups();
    }

    private void OnEnable()
    {
        // Ensure initial state is set immediately when enabled, 
        // preventing one-frame flashes before Start() runs.
        if (fadeInOnStart)
        {
            SetAlpha(0f);
        }
        else if (fadeOutOnStart)
        {
            SetAlpha(1f);
        }
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            // Wait one frame to allow other scripts (like LivesManager) to populate the UI
            // before we start fading it in.
            isWaitingToFade = true;
            StartCoroutine(StartFadeInAfterDelay());
        }
        else if (fadeOutOnStart)
        {
            FadeOut();
        }
    }

    private void LateUpdate()
    {
        // Continuously enforce alpha 0 while waiting.
        // This catches any children instantiated during Start/Awake of other scripts.
        if (isWaitingToFade)
        {
            SetAlpha(0f);
        }
    }

    private IEnumerator StartFadeInAfterDelay()
    {
        // Wait until LivesManager has finished creating the icons.
        // If LivesManager is not present in this scene, do not block forever.
        float safetyTimeout = 2f;
        float t = 0f;

        while (t < safetyTimeout)
        {
            if (LivesManager.Instance == null) break;
            if (LivesManager.Instance.IsLivesUIReady) break;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        isWaitingToFade = false;

        // Re-enforce alpha 0 just in case
        SetAlpha(0f);
        FadeIn();
    }


    private void InitializeCanvasGroups()
    {
        _canvasGroups.Clear();
        foreach (var go in uiElements)
        {
            if (go != null)
            {
                var cg = go.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = go.AddComponent<CanvasGroup>();
                }
                _canvasGroups.Add(cg);
            }
        }
    }

    /// <summary>
    /// Fades all assigned elements from 0 to 1.
    /// </summary>
    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(0f, 1f));
    }

    /// <summary>
    /// Fades all assigned elements from 1 to 0.
    /// </summary>
    public void FadeOut()
    {
        StopAllCoroutines();
        StartCoroutine(FadeRoutine(1f, 0f));
    }

    /// <summary>
    /// Coroutine that handles the fading logic.
    /// </summary>
    public IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        
        SetAlpha(startAlpha);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            SetAlpha(currentAlpha);
            yield return null;
        }

        SetAlpha(endAlpha);
        onFadeComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        // 1. Apply to root CanvasGroups (Standard efficient way)
        foreach (var cg in _canvasGroups)
        {
            if (cg != null)
            {
                cg.alpha = alpha;
                bool isVisible = alpha > 0.05f;
                cg.blocksRaycasts = isVisible;
                cg.interactable = isVisible;
            }
        }

        // 2. Apply to child Graphics (Brute force way)
        if (fadeChildGraphics)
        {
            foreach (var go in uiElements)
            {
                if (go == null) continue;
                
                var graphics = go.GetComponentsInChildren<UnityEngine.UI.Graphic>();
                foreach (var g in graphics)
                {
                    Color c = g.color;
                    c.a = alpha;
                    g.color = c;
                }
            }
        }
    }
    
    /// <summary>
    /// Add an element at runtime.
    /// </summary>
    public void AddElement(GameObject go)
    {
        if (go != null && !uiElements.Contains(go))
        {
            uiElements.Add(go);
            
            // Register its CanvasGroup immediately
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            _canvasGroups.Add(cg);
        }
    }
}
