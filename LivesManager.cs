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
        this.maxLives = Mathf.Max(0, maxLives);
        currentLives = this.maxLives;
        currentIconSprite = iconSprite;

        // Clear existing
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
                // Fallback: Create simple Image object
                newIcon = new GameObject($"LifeIcon_{i}", typeof(RectTransform), typeof(Image));
                newIcon.transform.SetParent(livesContainer, false);
            }

            // Manual Positioning
            RectTransform rt = newIcon.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Align left-to-right from the container's anchor point.
                rt.anchoredPosition = new Vector2(i * iconSpacing, 0);
            }

            ApplySpriteToIcon(newIcon, currentIconSprite);
            lifeIcons.Add(newIcon);
        }
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

        Image img = iconObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
            img.enabled = true;
        }
    }

    private void OnSkinChanged(RocketSkin skin)
    {
        if (skin == null) return;
        UpdateLifeSprite(skin.uiSprite);
    }
}