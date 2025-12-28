using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Menu : MonoBehaviour
{
    [Tooltip("Optional: Assign a UIFader to handle the fade out transition.")]
    [SerializeField] private UIFader transitionFader;

    public void StartGame()
    {
        // Block starting while any skin carousel is visible/active.
        // if (SkinCarousel.AnyOpen())
        //    return;
        
        StartCoroutine(StartGameSequence());
    }

    private IEnumerator StartGameSequence()
    {
        // 1. Use the assigned Fader if it exists (Preferred)
        if (transitionFader != null)
        {
            yield return transitionFader.FadeRoutine(1f, 0f);
        }
        else
        {
            // 2. Legacy fallback: Find "Tap To Play" manually
            GameObject tapToPlay = GameObject.Find("Tap To Play (Words)");
            if (tapToPlay != null)
            {
                CanvasGroup cg = tapToPlay.GetComponent<CanvasGroup>();
                if (cg == null)
                {
                    cg = tapToPlay.AddComponent<CanvasGroup>();
                }

                float duration = 1.0f; 
                float elapsed = 0f;
                float startAlpha = cg.alpha;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime; 
                    cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                    yield return null;
                }
                cg.alpha = 0f;
            }
        }

        // Wait a small buffer if needed, or proceed immediately.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}