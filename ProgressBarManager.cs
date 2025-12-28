using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProgressBarManager : MonoBehaviour
{
    [Header("Anchor Settings")]
    [SerializeField] private bool useAnchorScript = true;
    [SerializeField] private AnchorGameObject.AnchorType anchorLocation = AnchorGameObject.AnchorType.MiddleRight;
    [SerializeField] private Vector3 anchorOffset = new Vector3(-50, 0, 0);

    [Header("UI References")]
    [Tooltip("The container for the progress bar. Icons will be children of this.")]
    [SerializeField] private RectTransform progressBarRectTransform;

    [Header("Icons")]
    [Tooltip("Prefab for the rocket tracker icon. If null, a default one will be created.")]
    [SerializeField] private GameObject rocketTrackerPrefab;
    [SerializeField] private GameObject checkpointIconPrefab;
    [SerializeField] private GameObject planetIconPrefab;
    
    [Header("Icon Scaling")]
    [Tooltip("Scale for the rocket tracker icon.")]
    [SerializeField] private float rocketIconScale = 0.05f;
    [Tooltip("Scale for planet icons.")]
    [SerializeField] private float planetScale = 0.5f;
    [Tooltip("Scale for checkpoint icons.")]
    [SerializeField] private float checkpointScale = 0.5f;

    [Header("Animation")]
    [Tooltip("Time in seconds for a planet icon to fade in.")]
    [SerializeField] private float planetFadeDuration = 3.0f;
    [Tooltip("Time in seconds for a checkpoint icon to fade in.")]
    [SerializeField] private float checkpointFadeDuration = 3.0f;

    [Header("Level Settings")]
    [Tooltip("If > 0, this overrides the game manager's calculated height. Use this to set a fixed finish line height (e.g. 5000).")]
    [SerializeField] private float manualMaxHeight = 0f;

    private GameManagerRS gameManager;
    private Transform rocketTransform;
    private SpriteRenderer rocketSpriteRenderer; // To sync animation
    private float startY;
    
    // Key: The world GameObject (Planet/Checkpoint). Value: The UI RectTransform.
    private Dictionary<MonoBehaviour, RectTransform> spawnedIcons = new Dictionary<MonoBehaviour, RectTransform>();
    // NEW: Cache for positions
    private Dictionary<MonoBehaviour, float> cachedNormalizedPositions = new Dictionary<MonoBehaviour, float>();
    private HashSet<MonoBehaviour> visibleIcons = new HashSet<MonoBehaviour>();
    
    private RectTransform activeRocketTrackerRect; 
    private Image activeRocketTrackerImage;

    private const float EARTH_ICON_OFFSET = -0.03f; // 3% below actual position to appear below rocket
    private const float PLANET_ICON_OFFSET = 0.1f; // Increased to test visual height and ensure planets are not too low

    private bool isPlayerDead = false;

    private RectTransform planetContainer;
    private RectTransform checkpointContainer;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManagerRS>();
        
        // Find Rocket Reference
        if (SkinManager.Instance != null && SkinManager.Instance.CurrentRocket != null)
        {
            rocketTransform = SkinManager.Instance.CurrentRocket.transform;
        }
        else
        {
            var p = GameObject.FindGameObjectWithTag("RocketShip");
            if (p != null) rocketTransform = p.transform;
        }

        if (rocketTransform != null) 
        {
            startY = rocketTransform.position.y;
            rocketSpriteRenderer = rocketTransform.GetComponentInChildren<SpriteRenderer>();
        }

        // Setup Anchor
        if (useAnchorScript && progressBarRectTransform != null)
        {
            var anchor = progressBarRectTransform.GetComponent<AnchorGameObject>();
            if (anchor == null) anchor = progressBarRectTransform.gameObject.AddComponent<AnchorGameObject>();
            anchor.anchorType = anchorLocation;
            anchor.anchorOffset = anchorOffset;
            anchor.executeInUpdate = true;
            anchor.enabled = true;
        }

        // Instantiate Rocket Tracker Icon
        if (progressBarRectTransform != null)
        {
            GameObject tracker;
            if (rocketTrackerPrefab != null)
            {
                tracker = Instantiate(rocketTrackerPrefab, progressBarRectTransform);
            }
            else
            {
                tracker = new GameObject("RocketTracker", typeof(Image));
                tracker.transform.SetParent(progressBarRectTransform, false);
            }
            
            activeRocketTrackerRect = EnsureRectTransform(tracker);
            activeRocketTrackerRect.localScale = Vector3.one * rocketIconScale; 
            
            activeRocketTrackerImage = activeRocketTrackerRect.GetComponent<Image>();
            if (activeRocketTrackerImage != null)
            {
                activeRocketTrackerImage.preserveAspect = true;
            }
        }
    }

    private void OnEnable()
    {
        GameEvents.PlayerCrashed += OnPlayerCrashed;
        GameEvents.PlayerRespawned += OnPlayerRespawned;
    }

    private void OnDisable()
    {
        GameEvents.PlayerCrashed -= OnPlayerCrashed;
        GameEvents.PlayerRespawned -= OnPlayerRespawned;
    }

    private void OnPlayerCrashed() => isPlayerDead = true;
    private void OnPlayerRespawned() => isPlayerDead = false;

    private void Update()
    {
        if (gameManager == null || rocketTransform == null || progressBarRectTransform == null) return;

        // 1. Determine Level Bounds
        float worldMin = startY;
        float worldMax = (manualMaxHeight > 0f) ? manualMaxHeight : gameManager.GetLevelMaxY();
        float levelRange = Mathf.Max(1f, worldMax - worldMin);

        float currentY = rocketTransform.position.y;
        float progress = Mathf.Clamp01((currentY - worldMin) / levelRange);
        
        // 2. Update Dynamic Icons
        // Pausing updates when player is dead ensures icons don't drift incorrectly
        if (!isPlayerDead)
        {
            // Ensure sorting containers exist
            if (planetContainer == null) planetContainer = CreateLayerContainer("PlanetContainer");
            if (checkpointContainer == null) checkpointContainer = CreateLayerContainer("CheckpointContainer");
            
            // Enforce explicit layer order: Planets (bottom), Checkpoints (middle), Rocket (top)
            if (planetContainer != null) 
            {
                planetContainer.SetAsFirstSibling();
                // Reset Z to 0
                Vector3 pPos = planetContainer.localPosition;
                pPos.z = 0f;
                planetContainer.localPosition = pPos;
            }
            
            if (checkpointContainer != null) 
            {
                checkpointContainer.SetSiblingIndex(planetContainer.GetSiblingIndex() + 1);
                
                // Move Checkpoints to Z=1 (requested) to potentially assist with custom sorting axes
                Vector3 cpPos = checkpointContainer.localPosition;
                cpPos.z = -1f;
                checkpointContainer.localPosition = cpPos;
            }

            UpdateDynamicIcons(gameManager.AllPlanets, worldMin, levelRange, planetIconPrefab, planetScale, planetFadeDuration, planetContainer);
            UpdateDynamicIcons(gameManager.AllCheckpoints, worldMin, levelRange, checkpointIconPrefab, checkpointScale, checkpointFadeDuration, checkpointContainer);
        }

        // 3. Update Rocket Icon
        if (activeRocketTrackerRect != null)
        {
            if (!activeRocketTrackerRect.gameObject.activeSelf) activeRocketTrackerRect.gameObject.SetActive(true);
            PlaceIconOnBar(activeRocketTrackerRect, progress);
            
            if (activeRocketTrackerImage != null && rocketSpriteRenderer != null)
            {
                activeRocketTrackerImage.sprite = rocketSpriteRenderer.sprite;
            }
            
            if (activeRocketTrackerRect.localScale.x != rocketIconScale)
            {
                activeRocketTrackerRect.localScale = Vector3.one * rocketIconScale;
            }
            
            // Rocket must be absolutely last to be on top of everything
            activeRocketTrackerRect.SetAsLastSibling();
        }
    }

    private void UpdateDynamicIcons<T>(HashSet<T> worldObjects, float worldMin, float levelRange, GameObject prefab, float scale, float fadeDuration, Transform container) where T : MonoBehaviour
    {
        if (prefab == null) return;
        if (spawnedIcons.Count > 200) return;

        foreach (var obj in worldObjects)
        {
            if (obj == null) continue;

            bool isEarth = (obj is CelestialObjectController) && obj.CompareTag("Earth");

            // --- Instantiation ---
            if (!spawnedIcons.ContainsKey(obj))
            {
                GameObject newIcon = Instantiate(prefab, container);
                RectTransform rt = EnsureRectTransform(newIcon);

                if (rt != null)
                {
                    rt.localScale = Vector3.one * scale;
                    
                    // Add CanvasGroup for robust alpha control
                    CanvasGroup cg = newIcon.GetComponent<CanvasGroup>();
                    if (cg == null) cg = newIcon.AddComponent<CanvasGroup>();
                    
                    Image img = newIcon.GetComponent<Image>();
                    // Only automatically add an Image if we plan to assign a planet sprite to it.
                    // Otherwise, we rely on the prefab's internal visuals (e.g., child objects).
                    if (img == null && obj is CelestialObjectController) 
                    {
                        img = newIcon.AddComponent<Image>();
                    }
                    
                    if (obj is CelestialObjectController)
                    {
                        SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null && img != null)
                        {
                            img.sprite = sr.sprite;
                            img.preserveAspect = true;
                        }
                    }
                    else if (obj is Checkpoint)
                    {
                        // If the prefab had an Image but no sprite, it renders as a white square.
                        // Disable it to reveal any child visuals or avoid the white box.
                        if (img != null && img.sprite == null)
                        {
                            img.enabled = false;
                        }
                    }

                    // Force START INVISIBLE (Alpha 0) immediately unless it's Earth
                    cg.alpha = isEarth ? 1f : 0f;
                    
                    // Also strictly disable the GameObject if not Earth, to prevent any "white square" or visible lines from showing up early
                    newIcon.SetActive(isEarth);

                    spawnedIcons.Add(obj, rt);
                    
                    if (isEarth) visibleIcons.Add(obj);
                }
                else
                {
                    Destroy(newIcon);
                    continue; 
                }
            }

            // --- Positioning ---
            RectTransform iconRect = spawnedIcons[obj];
            
            // Fix: Ensure the icon is in the correct container (handles hot-reload or legacy icons)
            if (iconRect.transform.parent != container)
            {
                iconRect.transform.SetParent(container, false);
            }

            // Fix: Disable any Canvas sorting overrides that might disrupt hierarchy-based sorting
            Canvas iconCanvas = iconRect.GetComponent<Canvas>();
            if (iconCanvas != null && iconCanvas.overrideSorting)
            {
                iconCanvas.overrideSorting = false;
            }

            CanvasGroup canvasGroup = iconRect.GetComponent<CanvasGroup>();
            
            float normalizedPos = 0f;

            // Use cached position if available, otherwise calculate and cache it.
            if (cachedNormalizedPositions.ContainsKey(obj))
            {
                normalizedPos = cachedNormalizedPositions[obj];
            }
            else
            {
                float predictedY = 0f;
                float currentWorldY = obj.transform.position.y;

                if (obj is CelestialObjectController planet)
                {
                    // Predict interception point
                    float f = planet.relativeSpeedFactor;
                    // If f >= 1, the planet moves faster than or equal to rocket speed, no interception from below.
                    // We use 0.99f as a safety margin to avoid divide by zero.
                    if (f < 0.99f)
                    {
                        float rocketY = rocketTransform.position.y;
                        // Formula: InterceptY = R + (P - R) / (1 - f)
                        float dist = currentWorldY - rocketY;
                        predictedY = rocketY + (dist / (1f - f));
                    }
                    else
                    {
                        // Fallback to current position if it moves too fast (shouldn't happen for normal planets)
                        predictedY = currentWorldY; 
                    }
                }
                else
                {
                    // Static objects (Checkpoints)
                    predictedY = currentWorldY;
                }

                normalizedPos = Mathf.Clamp01((predictedY - worldMin) / levelRange);
                
                if (isEarth)
                {
                    // Apply manual offset to put Earth slightly below the rocket at the start
                    normalizedPos += EARTH_ICON_OFFSET; 
                }
                else if (obj is CelestialObjectController)
                {
                    normalizedPos += PLANET_ICON_OFFSET;
                }
                
                // Cache the calculated position so it doesn't change on respawn
                cachedNormalizedPositions[obj] = normalizedPos;
            }

            PlaceIconOnBar(iconRect, normalizedPos);

            // --- Visibility Check ---
            bool shouldShow = false;

            if (isEarth)
            {
                shouldShow = true;
            }
            else if (obj is Checkpoint cp)
            {
                if (cp.IsActivated) shouldShow = true;
            }
            else if (obj is CelestialObjectController)
            {
                // Check if the planet has already been made visible.
                // If it has, it should remain visible on the progress bar.
                if (visibleIcons.Contains(obj))
                {
                    shouldShow = true;
                }
                else
                {
                    // Otherwise, determine initial visibility:
                    // Show planet if it is within 100 units below or ahead of the rocket's current Y position.
                    shouldShow = obj.transform.position.y >= (rocketTransform.position.y);
                }
            }

            // --- Apply Visibility ---
            if (shouldShow)
            {
                // Ensure object is active so it can render and fade
                if (!iconRect.gameObject.activeSelf) iconRect.gameObject.SetActive(true);

                if (!visibleIcons.Contains(obj))
                {
                    visibleIcons.Add(obj);
                    // Fade in
                    StartCoroutine(FadeInIcon(canvasGroup, fadeDuration));
                }
                // If it's Earth, ensure it stays visible
                if (isEarth && canvasGroup.alpha < 1f) canvasGroup.alpha = 1f;
            }
            else
            {
                // Ensure invisible
                if (visibleIcons.Contains(obj))
                {
                    visibleIcons.Remove(obj);
                    canvasGroup.alpha = 0f;
                    // Explicitly disable to guarantee invisibility
                    iconRect.gameObject.SetActive(false);
                }
                else
                {
                    // Continuous enforcement for safety (prevents glitches)
                    if (iconRect.gameObject.activeSelf) iconRect.gameObject.SetActive(false);
                }
            }
        }
    }

    private IEnumerator FadeInIcon(CanvasGroup cg, float duration)
    {
        if (duration <= 0f)
        {
            cg.alpha = 1f;
            yield break;
        }

        float elapsed = 0f;
        cg.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        cg.alpha = 1f;
    }

    private RectTransform EnsureRectTransform(GameObject obj)
    {
        if (obj == null) return null;
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = obj.AddComponent<RectTransform>();
        }
        
        rt.pivot = new Vector2(0.5f, 0.5f);
        return rt;
    }

    private void PlaceIconOnBar(RectTransform icon, float normalizedY)
    {
        if (icon == null) return;

        float barHeight = progressBarRectTransform.rect.height;
        float yPos = (normalizedY - 0.5f) * barHeight;
        
        icon.anchoredPosition = new Vector2(0f, yPos);
        
        // Ensure Z is zero to prevent 3D sorting issues in Camera space
        Vector3 locPos = icon.localPosition;
        if (Mathf.Abs(locPos.z) > 0.001f)
        {
            locPos.z = 0f;
            icon.localPosition = locPos;
        }
    }

    private RectTransform CreateLayerContainer(string name)
    {
        if (progressBarRectTransform == null) return null;
        
        Transform existing = progressBarRectTransform.Find(name);
        if (existing != null) return existing.GetComponent<RectTransform>();

        GameObject container = new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(progressBarRectTransform, false);
        RectTransform rt = container.GetComponent<RectTransform>();
        
        // Stretch to fill parent
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        
        return rt;
    }
}