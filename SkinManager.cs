using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;

// Central authority for "which rocket is active".
// Supports two modes:
// - Legacy mode: toggle activeSelf on in-scene rocket objects.
// - Prefab mode: spawn the selected RocketSkin prefab.
public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Selection")]
    [SerializeField] private int defaultSkinIndex = 0;
    [SerializeField] private bool clampSelectionToAvailable = true;

    [Header("Data-Driven Skins (Optional)")]
    [SerializeField] private List<RocketSkin> skins = new List<RocketSkin>();

    [Header("Prefab Spawning Mode (Optional)")]
    [SerializeField] private bool usePrefabSpawning = false;
    [SerializeField] private Transform spawnPoint;

    [Header("Legacy Scene Mode (Optional)")]
    [Tooltip("If prefab spawning is off, SkinManager will activate exactly one of these GameObjects.")]
    [SerializeField] private List<GameObject> legacyRocketObjects = new List<GameObject>();

    private GameObject _spawnedRocket;
    private int _currentIndex;

    public int CurrentIndex => _currentIndex;
    public RocketSkin CurrentSkin => IsValidSkinIndex(_currentIndex) ? skins[_currentIndex] : null;
    public GameObject CurrentRocket => ResolveCurrentRocket();
    public Transform CurrentRocketTransform => CurrentRocket != null ? CurrentRocket.transform : null;
    public Rigidbody2D CurrentRocketRigidbody => CurrentRocket != null ? CurrentRocket.GetComponent<Rigidbody2D>() : null;

    private static bool IsMenuScene()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.buildIndex == 0) return true;

        string n = scene.name;
        if (string.IsNullOrEmpty(n)) return false;
        n = n.ToLowerInvariant();
        return n.Contains("pregame") || n.Contains("menu") || n.Contains("title") || n.Contains("main");
    }

    private void Awake()
    {
        // Singleton + persist across scenes so the selection cannot reset unexpectedly.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        _currentIndex = PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, defaultSkinIndex);
        ApplySelection(_currentIndex, fireEvent: false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }


    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When entering gameplay scenes, re-apply the saved selection.
        _currentIndex = PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, defaultSkinIndex);

        // In gameplay scenes we clamp (if desired) after we have scene objects.
        if (!IsMenuScene() && clampSelectionToAvailable)
            _currentIndex = ClampToAvailable(_currentIndex);

        PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, _currentIndex);

        ApplySelection(_currentIndex, fireEvent: true);
    }

    public bool HasSkins => GetMaxSelectableIndex() >= 0;
    public int SkinCount => skins.Count;
    public RocketSkin GetSkinAt(int index) => IsValidSkinIndex(index) ? skins[index] : null;

    public void SelectIndex(int index)
    {
        if (!IsMenuScene() && clampSelectionToAvailable && HasSkins)
            index = ClampToAvailable(index);

        if (index == _currentIndex)
        {
            // If user hits Select without changing, still notify listeners.
            NotifySkinChanged();
            return;
        }

        _currentIndex = index;
        PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, _currentIndex);

        ApplySelection(_currentIndex, fireEvent: true);
    }

    public void NotifySkinChanged()
    {
        GameEvents.RaiseSkinChanged(CurrentSkin);
    }

    public void Next()
    {
        int max = GetMaxSelectableIndex();
        if (max < 0) return;
        SelectIndex(Mathf.Min(_currentIndex + 1, max));
    }

    public void Previous()
    {
        SelectIndex(Mathf.Max(_currentIndex - 1, 0));
    }

    public bool IsUnlocked(int index)
    {
        RocketSkin skin = IsValidSkinIndex(index) ? skins[index] : null;
        if (skin == null) return true;

        if (skin.lockOverride == LockOverride.ForceUnlocked)
            return true;
        if (skin.lockOverride == LockOverride.ForceLocked)
            return false;

        int bestScore = 0;
        string best = PlayerPrefs.GetString(GameConstants.PrefHighScore, "0");
        int.TryParse(best, NumberStyles.Integer, CultureInfo.InvariantCulture, out bestScore);
        return skin.unlockRequirement.IsUnlocked(bestScore);
    }

    public void RegisterLegacyRocket(GameObject rocket)
    {
        if (rocket == null) return;
        if (!legacyRocketObjects.Contains(rocket))
            legacyRocketObjects.Add(rocket);
    }

    public void RegisterSpawnedRocket(GameObject rocket)
    {
        if (rocket == null) return;
        
        // If we had a previous spawned rocket that is different, we might want to clean it up?
        // For now, just overwrite the reference since the scene likely just loaded or we are respawning.
        _spawnedRocket = rocket;
    }

    private void ApplySelection(int index, bool fireEvent)
    {
        // In menu scenes we do not spawn/toggle gameplay rockets.
        if (IsMenuScene())
        {
            if (_spawnedRocket != null)
            {
                Destroy(_spawnedRocket);
                _spawnedRocket = null;
            }
        }
        else
        {
            GameObject resolvedCurrentRocket = ResolveCurrentRocket();
            bool updateSkinDetected = DoesRocketUseUpdateSkin(resolvedCurrentRocket);

            if (usePrefabSpawning)
            {
                SpawnFromSkin(index);
            }
            else if (!updateSkinDetected)
            {
                ActivateLegacyRocket(index);
            }
        }

        if (fireEvent)
            GameEvents.RaiseSkinChanged(CurrentSkin);
    }

    private void SpawnFromSkin(int index)
    {
        if (_spawnedRocket != null)
            Destroy(_spawnedRocket);

        RocketSkin skin = IsValidSkinIndex(index) ? skins[index] : null;
        if (skin == null || skin.rocketPrefab == null)
        {
            _spawnedRocket = null;
            return;
        }

        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        _spawnedRocket = Instantiate(skin.rocketPrefab, pos, rot);
    }

    private void ActivateLegacyRocket(int index)
    {
        if (legacyRocketObjects == null || legacyRocketObjects.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, legacyRocketObjects.Count - 1);
        for (int i = 0; i < legacyRocketObjects.Count; i++)
        {
            if (legacyRocketObjects[i] == null) continue;
            legacyRocketObjects[i].SetActive(i == index);
        }
    }

    private GameObject ResolveCurrentRocket()
    {
        // If we have a spawned rocket (either via internal prefab spawning or external registration), use it.
        if (_spawnedRocket != null)
            return _spawnedRocket;
            
        if (usePrefabSpawning)
            return _spawnedRocket; // Should be null here if caught above, but keeping structure.

        if (legacyRocketObjects == null || legacyRocketObjects.Count == 0)
            return null;

        int index = Mathf.Clamp(_currentIndex, 0, legacyRocketObjects.Count - 1);
        GameObject candidate = legacyRocketObjects[index];
        if (candidate != null && candidate.activeInHierarchy)
            return candidate;

        for (int i = 0; i < legacyRocketObjects.Count; i++)
        {
            GameObject go = legacyRocketObjects[i];
            if (go != null && go.activeInHierarchy)
                return go;
        }

        return candidate;
    }

    private int ClampToAvailable(int index)
    {
        int max = GetMaxSelectableIndex();
        if (max < 0) return 0;
        return Mathf.Clamp(index, 0, max);
    }

    private int GetMaxSelectableIndex()
    {
        if (skins != null && skins.Count > 0) return skins.Count - 1;
        if (legacyRocketObjects != null && legacyRocketObjects.Count > 0) return legacyRocketObjects.Count - 1;
        return -1;
    }

    private bool IsValidSkinIndex(int index)
    {
        return skins != null && index >= 0 && index < skins.Count;
    }

    private bool DoesRocketUseUpdateSkin(GameObject rocket)
    {
        return rocket != null && rocket.GetComponent<UpdateSkin>() != null;
    }
}
