using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance { get; private set; }

    [Header("UI Setup")]
    [Tooltip("The parent container. Ensure this RectTransform is anchored appropriately (e.g., Top-Left or Top-Right).")]
    [SerializeField] private Transform livesContainer;
    [Tooltip("The prefab or sprite to use for the life icon.")]
    [SerializeField] private GameObject lifeIconPrefab;
    [Tooltip("Distance between life icons in pixels.")]
    [SerializeField] private float iconSpacing = 50f;

    [Header("Anchor Settings")]
    [Tooltip("If true, attaches an AnchorGameObject script to the container to lock it to a world position (e.g. BottomLeft).")]
    [SerializeField] private bool useAnchorScript = false;
    [SerializeField] private AnchorGameObject.AnchorType anchorLocation = AnchorGameObject.AnchorType.BottomLeft;
    [SerializeField] private Vector3 anchorOffset = Vector3.zero;

    private List<GameObject> lifeIcons = new List<GameObject>();
    private int maxLives;
    private int currentLives;
    private Sprite currentIconSprite;
    private RocketSkin currentSkin;

    public bool IsLivesUIReady { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Apply Anchor Configuration if requested
        if (useAnchorScript && livesContainer != null)
        {
            AnchorGameObject anchor = livesContainer.GetComponent<AnchorGameObject>();
            if (anchor == null)
            {
                anchor = livesContainer.gameObject.AddComponent<AnchorGameObject>();
            }

            anchor.anchorType = anchorLocation;
            anchor.anchorOffset = anchorOffset;
            anchor.executeInUpdate = true;
            
            // Force an immediate update so it jumps to position
            anchor.enabled = true;
        }
    }

    private void OnEnable()
    {
        GameEvents.SkinChanged += OnSkinChanged;
    }

    private void OnDisable()
    {
        GameEvents.SkinChanged -= OnSkinChanged;
    }

    public void SetupLives(int maxLives, Sprite iconSprite)
    {
        IsLivesUIReady = false;

        this.maxLives = Mathf.Max(0, maxLives);
        currentLives = this.maxLives;
        currentIconSprite = iconSprite;

        // Clear existing...
        foreach (var icon in lifeIcons)
        {
            if (icon != null) Destroy(icon);
        }
        lifeIcons.Clear();

        if (livesContainer == null) return;

        for (int i = 0; i < this.maxLives; i++)
        {
            GameObject newIcon = null;
            if (lifeIconPrefab != null)
            {
                newIcon = Instantiate(lifeIconPrefab, livesContainer);
            }
            else
            {
                newIcon = new GameObject($"LifeIcon_{i}", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                newIcon.transform.SetParent(livesContainer, false);
            }

            RectTransform rt = newIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * iconSpacing, 0);
            }

            ApplySpriteToIcon(newIcon, currentIconSprite);
            lifeIcons.Add(newIcon);
        }

        // If we already have a skin, re-apply to enforce opacity/size immediately.
        if (currentSkin != null)
        {
            var sprite = currentSkin.lifeIconSprite != null ? currentSkin.lifeIconSprite : currentSkin.uiSprite;
            UpdateLifeSprite(sprite);
        }


        IsLivesUIReady = true;
    }

    public void LoseLife()
    {
        if (currentLives <= 0) return;

        currentLives = Mathf.Max(0, currentLives - 1);

        // Remove the last item (Right-most in the list)
        if (lifeIcons.Count > 0)
        {
            int targetIndex = lifeIcons.Count - 1;
            GameObject targetIcon = lifeIcons[targetIndex];

            if (targetIcon != null)
            {
                Destroy(targetIcon);
            }
            
            lifeIcons.RemoveAt(targetIndex);
        }
    }

    public void UpdateLifeSprite(Sprite iconSprite)
    {
        currentIconSprite = iconSprite;
        if (currentIconSprite == null) return;

        for (int i = 0; i < lifeIcons.Count; i++)
        {
            ApplySpriteToIcon(lifeIcons[i], currentIconSprite);
        }
    }

    private void ApplySpriteToIcon(GameObject iconObj, Sprite sprite)
    {
        if (iconObj == null || sprite == null) return;

        float alpha = 1f;
        Vector2 size = new Vector2(64f, 64f);

        if (currentSkin != null)
        {
            alpha = Mathf.Clamp01(currentSkin.lifeIconOpacity);
            size = currentSkin.lifeIconSize;
        }

        // Add or get a CanvasGroup on the icon root and drive opacity through it.
        // This is harder for other code to accidentally override.
        var cg = iconObj.GetComponent<CanvasGroup>();
        if (cg == null) cg = iconObj.AddComponent<CanvasGroup>();
        cg.alpha = alpha;

        // Apply sprite to the visible Images (root + children)
        Image[] images = iconObj.GetComponentsInChildren<Image>(true);
        if (images == null || images.Length == 0) return;

        foreach (var img in images)
        {
            if (img == null) continue;

            img.sprite = sprite;
            img.preserveAspect = true;
            img.enabled = true;

            // Keep image color alpha at 1 so CanvasGroup is the single source of truth.
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }

        images[0].rectTransform.sizeDelta = size;
    }





    private void OnSkinChanged(RocketSkin skin)
    {
        if (skin == null) return;

        currentSkin = skin;

        // Prefer the per-skin life icon, fall back to uiSprite if not assigned
        var sprite = skin.lifeIconSprite != null ? skin.lifeIconSprite : skin.uiSprite;
        UpdateLifeSprite(sprite);
    }


}