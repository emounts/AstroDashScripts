using System.Collections;
using UnityEngine;

public class EarthRespawner : MonoBehaviour
{
    [Header("Template")]
    [Tooltip("Best: assign the EARTH PREFAB asset here. If you assign a scene Earth by accident, this script will clone it once at startup.")]
    [SerializeField] private GameObject earthTemplate;

    [Tooltip("Optional parent for the respawned Earth. Leave blank to spawn at scene root.")]
    [SerializeField] private Transform earthParent;

    [Header("Bottom respawn detection")]
    [Tooltip("If set, uses this as the bottom respawn reference. If blank, it will search for a GameObject named 'RocketSpawnPoint'.")]
    [SerializeField] private Transform rocketSpawnPoint;

    [Tooltip("How close to the spawnpoint Y counts as 'bottom respawn'.")]
    [SerializeField] private float bottomRespawnToleranceY = 2.0f;

    [Header("Spawn transform")]
    [SerializeField] private bool respawnAtInitialEarthPosition = true;

    private Vector3 _initialEarthPos;
    private Quaternion _initialEarthRot;
    private Vector3 _initialEarthScale;

    private GameObject _runtimeTemplate; // safe template if someone assigns a scene object

    private void Awake()
    {
        CacheInitialEarthTransform();
        ResolveSpawnPoint();
        BuildRuntimeTemplateIfNeeded();
    }

    private void OnEnable()
    {
        GameEvents.PlayerRespawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        GameEvents.PlayerRespawned -= OnPlayerRespawned;
    }

    private void OnPlayerRespawned()
    {
        StartCoroutine(RespawnCheckNextFrame());
    }

    private IEnumerator RespawnCheckNextFrame()
    {
        yield return null; // wait for rocket reposition / respawn logic to finish

        ResolveSpawnPoint();

        // If Earth still exists, do nothing
        if (FindExistingEarth() != null) yield break;

        Transform rocket = ResolveRocket();
        if (rocket == null) yield break;

        float referenceY = rocketSpawnPoint != null ? rocketSpawnPoint.position.y : rocket.position.y;
        bool isBottomRespawn = rocketSpawnPoint != null &&
                              rocket.position.y <= (referenceY + bottomRespawnToleranceY);

        // If we cannot determine bottom respawn (no spawnpoint found), be permissive and respawn if missing
        // This prevents "no earth ever" bugs.
        if (rocketSpawnPoint != null && !isBottomRespawn) yield break;

        GameObject templateToUse = _runtimeTemplate != null ? _runtimeTemplate : earthTemplate;
        if (templateToUse == null)
        {
            Debug.LogWarning("EarthRespawner: No template available. Assign an Earth prefab to Earth Template.");
            yield break;
        }

        Vector3 spawnPos = respawnAtInitialEarthPosition ? _initialEarthPos : templateToUse.transform.position;
        Quaternion spawnRot = respawnAtInitialEarthPosition ? _initialEarthRot : templateToUse.transform.rotation;

        GameObject earth = Instantiate(templateToUse, spawnPos, spawnRot, earthParent);
        earth.transform.localScale = _initialEarthScale;

        // Ensure progress bar sees it correctly
        earth.tag = "Earth";
    }

    private void ResolveSpawnPoint()
    {
        if (rocketSpawnPoint != null) return;

        var sp = GameObject.Find("RocketSpawnPoint");
        if (sp != null) rocketSpawnPoint = sp.transform;
    }

    private void CacheInitialEarthTransform()
    {
        // Prefer finding Earth in scene at startup to cache where it should respawn
        GameObject earth = FindExistingEarth();
        if (earth != null)
        {
            _initialEarthPos = earth.transform.position;
            _initialEarthRot = earth.transform.rotation;
            _initialEarthScale = earth.transform.localScale;

            // If no template assigned, use whatever we found (we will protect it via runtime clone)
            if (earthTemplate == null) earthTemplate = earth;

            // If parent not set, keep original parent
            if (earthParent == null) earthParent = earth.transform.parent;
        }
        else
        {
            // Safe defaults
            _initialEarthPos = Vector3.zero;
            _initialEarthRot = Quaternion.identity;
            _initialEarthScale = Vector3.one;
        }
    }

    private void BuildRuntimeTemplateIfNeeded()
    {
        if (earthTemplate == null) return;

        // If the assigned template is a scene object, it will become Missing when destroyed.
        // Clone it once and keep the clone inactive as a safe template.
        if (earthTemplate.scene.IsValid())
        {
            _runtimeTemplate = Instantiate(earthTemplate);
            _runtimeTemplate.name = earthTemplate.name + "_RuntimeTemplate";
            _runtimeTemplate.SetActive(false);
            DontDestroyOnLoad(_runtimeTemplate);
            _runtimeTemplate.transform.SetParent(null, false);
        }
    }

    private Transform ResolveRocket()
    {
        if (SkinManager.Instance != null && SkinManager.Instance.CurrentRocketTransform != null)
            return SkinManager.Instance.CurrentRocketTransform;

        var go = GameObject.FindGameObjectWithTag("RocketShip");
        return go != null ? go.transform : null;
    }

    private GameObject FindExistingEarth()
    {
        // Tag-based (preferred)
        try
        {
            var tagged = GameObject.FindGameObjectWithTag("Earth");
            if (tagged != null) return tagged;
        }
        catch { }

        // Fallback: name contains earth
        var all = FindObjectsByType<CelestialObjectController>(FindObjectsSortMode.None);
        foreach (var c in all)
        {
            if (c == null) continue;
            if (c.name.ToLower().Contains("earth")) return c.gameObject;
        }

        return null;
    }
}
