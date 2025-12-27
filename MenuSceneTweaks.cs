using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Enforces a fully stationary rocket in menu/pregame scenes.
/// This requires no scene wiring.
/// </summary>
public static class MenuSceneTweaks
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallSceneHook()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AfterSceneLoad()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!IsPregameScene(sceneName))
            return;

        FreezePregameRockets();
    }

    private static void OnSceneChanged(Scene _, Scene newScene)
    {
        if (!IsPregameScene(newScene.name))
            return;

        FreezePregameRockets();
    }

    private static bool IsPregameScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return false;
        // Broad match so tweaks survive scene renames.
        string n = sceneName.ToLowerInvariant();
        return n == "pregame ui" ||
               n.Contains("pregame") ||
               n.Contains("menu") ||
               n.Contains("title") ||
               n.Contains("main");
    }

    private static void FreezePregameRockets()
    {
        // Freeze any RocketController-driven motion in the pregame/menu scene.
#if UNITY_2023_1_OR_NEWER
        RocketController[] controllers = Object.FindObjectsByType<RocketController>(FindObjectsSortMode.None);
#else
        RocketController[] controllers = Object.FindObjectsOfType<RocketController>();
#endif
        for (int i = 0; i < controllers.Length; i++)
        {
            RocketController c = controllers[i];
            if (c == null) continue;
            c.enableMovement = false;
            c.enabled = false;
            if (c.rb != null)
            {
                c.rb.linearVelocity = Vector2.zero;
                c.rb.angularVelocity = 0f;
                c.rb.simulated = false;
            }
        }

        // Also freeze any rigidbodies tagged RocketShip as an extra safety net.
        GameObject[] rockets = GameObject.FindGameObjectsWithTag("RocketShip");
        for (int i = 0; i < rockets.Length; i++)
        {
            GameObject go = rockets[i];
            if (go == null) continue;
            Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = false;
            }
        }
    }
}
