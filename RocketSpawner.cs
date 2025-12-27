using UnityEngine;

public class RocketSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private SkinManager skinManager;

    private GameObject spawnedRocket;

    private void Awake()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
    }

    private void Start()
    {
        SpawnSelectedRocket();
    }

    public void SpawnSelectedRocket()
    {
        if (spawnedRocket != null)
            Destroy(spawnedRocket);

        if (skinManager == null)
        {
            Debug.LogError("[RocketSpawner] SkinManager.Instance is null. Make sure SkinManager is DontDestroyOnLoad.");
            return;
        }

        RocketSkin skin = skinManager.CurrentSkin;
        if (skin == null || skin.rocketPrefab == null)
        {
            Debug.LogError("[RocketSpawner] Selected skin or its rocketPrefab is null.");
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        spawnedRocket = Instantiate(skin.rocketPrefab, pos, rot);
        spawnedRocket.name = $"PlayerRocket_{skin.name}";

        if (skinManager != null)
        {
            skinManager.RegisterSpawnedRocket(spawnedRocket);
        }

        ForceGameplayEnabled(spawnedRocket);

        Debug.Log($"[RocketSpawner] Spawned {spawnedRocket.name} at index {skinManager.CurrentIndex}");
    }

    private void ForceGameplayEnabled(GameObject rocket)
    {
        // Make sure physics + movement are on (in case prefab had them toggled off in menu work)
        var rb2d = rocket.GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.simulated = true;

        var rc = rocket.GetComponent<RocketController>();
        if (rc != null)
        {
            rc.enabled = true;
            rc.enableMovement = true;
        }

        var mv1 = rocket.GetComponent<RocketMovement1>();
        if (mv1 != null) mv1.enabled = true;

        var mvMenu = rocket.GetComponent<RocketMovementMenu>();
        if (mvMenu != null) mvMenu.enabled = false; // menu mover off in gameplay
    }
}
