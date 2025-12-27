using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns and positions 3 preview items (prev, selected, next) when the skin menu is open.
/// It enables PreviewItemContainer on open, generates preview clones on first open,
/// and repositions them on open and on selection changes.
/// </summary>
public class SkinCarousel : MonoBehaviour
{
    private static readonly HashSet<SkinCarousel> ActiveCarousels = new HashSet<SkinCarousel>();
    public static bool SelectionOpen => ActiveCarousels.Count > 0;

    [Header("Dependencies")]
    [SerializeField] private SkinManager skinManager;

    [Header("Preview Objects (Dynamic Generation)")]
    [SerializeField] private Transform previewItemContainer;

    [Tooltip("Optional: used only if a skin has no rocket prefab and you want a UI Image fallback.")]
    [SerializeField] private Image previewImagePrefab;

    [Header("Preview Item Settings")]
    [SerializeField] private Vector3 baseItemScale = Vector3.one;
    [SerializeField] private int previewSortingOrder = 10;

    [Header("Layout Anchors (Optional)")]
    [Tooltip("If set, these world transforms define the target positions for left/center/right preview items.")]
    [SerializeField] private Transform leftTransform;

    [SerializeField] private Transform centerTransform;
    [SerializeField] private Transform rightTransform;

    [Header("Fallback Local Layout (used if anchors are not set)")]
    [SerializeField] private Vector3 centerLocalPosition = new Vector3(0f, 2.50f, 0f);
    [SerializeField] private Vector3 leftLocalPosition = new Vector3(-2.20f, 1.20f, 0f);
    [SerializeField] private Vector3 rightLocalPosition = new Vector3(2.20f, 1.20f, 0f);

    [SerializeField] private Vector3 centerScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private Vector3 sideScale = new Vector3(1.2f, 1.2f, 1.2f);

    [Header("Tint")]
    [SerializeField] private Color centerTint = Color.white;
    [SerializeField] private Color sideTint = Color.gray;

    private readonly List<Transform> previewItems = new List<Transform>();
    private bool generated;

    private void Awake()
    {
        if (skinManager == null)
            skinManager = SkinManager.Instance;
    }

    private void OnEnable()
    {
        ActiveCarousels.Add(this);

        EnsurePreviewContainerEnabled();

        // Generate preview clones the first time the menu becomes active.
        if (!generated)
        {
            GeneratePreviewItems();
            generated = true;
        }

        GameEvents.SkinChanged += OnSkinChanged;

        // Always refresh when menu opens so positions update after activation.
        Refresh();
    }

    private void OnDisable()
    {
        ActiveCarousels.Remove(this);
        GameEvents.SkinChanged -= OnSkinChanged;
    }

    private void EnsurePreviewContainerEnabled()
    {
        if (previewItemContainer == null) return;

        if (!previewItemContainer.gameObject.activeSelf)
            previewItemContainer.gameObject.SetActive(true);
    }

    private void GeneratePreviewItems()
    {
        if (previewItemContainer == null)
        {
            Debug.LogWarning("[SkinCarousel] PreviewItemContainer is not assigned.");
            return;
        }

        if (skinManager == null)
        {
            Debug.LogWarning("[SkinCarousel] SkinManager is not assigned.");
            return;
        }

        if (!skinManager.HasSkins)
        {
            Debug.LogWarning("[SkinCarousel] SkinManager reports no skins.");
            return;
        }

        // Clear old clones if any
        for (int i = 0; i < previewItems.Count; i++)
        {
            if (previewItems[i] != null)
                Destroy(previewItems[i].gameObject);
        }
        previewItems.Clear();

        // Create one preview object per skin index (we will activate only 3 at a time)
        for (int i = 0; i < skinManager.SkinCount; i++)
        {
            RocketSkin skin = skinManager.GetSkinAt(i);
            if (skin == null)
            {
                previewItems.Add(null);
                continue;
            }

            GameObject previewGo = null;

            // Preferred: spawn the rocket prefab
            if (skin.rocketPrefab != null)
            {
                previewGo = Instantiate(skin.rocketPrefab, previewItemContainer, worldPositionStays: false);

                // Start neutral, Refresh() will place correctly
                previewGo.transform.localPosition = Vector3.zero;
                previewGo.transform.localRotation = Quaternion.identity;
                previewGo.transform.localScale = baseItemScale;

                // Sorting order for all SpriteRenderers so it appears above background/UI
                var renderers = previewGo.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in renderers)
                    sr.sortingOrder = previewSortingOrder;

                // Disable physics and gameplay movement scripts for previews
                var rb2d = previewGo.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                {
                    rb2d.simulated = false;
                    rb2d.linearVelocity = Vector2.zero;
                    rb2d.angularVelocity = 0f;
                }

                var rocketController = previewGo.GetComponent<RocketController>();
                if (rocketController != null)
                {
                    rocketController.enableMovement = false;
                    rocketController.enabled = false;
                }

                var move1 = previewGo.GetComponent<RocketMovement1>();
                if (move1 != null) move1.enabled = false;

                var moveMenu = previewGo.GetComponent<RocketMovementMenu>();
                if (moveMenu != null) moveMenu.enabled = false;

                var cols2D = previewGo.GetComponentsInChildren<Collider2D>(true);
                foreach (var c in cols2D) c.enabled = false;

                var cols3D = previewGo.GetComponentsInChildren<Collider>(true);
                foreach (var c in cols3D) c.enabled = false;

                // Keep Animator enabled so idle animations play if present
                var anim = previewGo.GetComponent<Animator>();
                if (anim != null) anim.enabled = true;
            }
            else if (skin.uiSprite != null && previewImagePrefab != null)
            {
                // Fallback: UI image preview if no prefab exists
                Image img = Instantiate(previewImagePrefab, previewItemContainer);
                img.sprite = skin.uiSprite;
                img.preserveAspect = true;
                previewGo = img.gameObject;
                previewGo.transform.localScale = baseItemScale;
            }
            else
            {
                // Nothing to show for this skin
                previewGo = new GameObject($"SkinPreview_{i}_Empty");
                previewGo.transform.SetParent(previewItemContainer, false);
                previewGo.SetActive(false);
            }

            previewGo.name = $"SkinPreview_{i}";
            previewGo.SetActive(false);
            previewItems.Add(previewGo.transform);
        }

        Debug.Log($"[SkinCarousel] Generated {previewItems.Count} preview items.");
    }

    public void Next()
    {
        if (skinManager == null) return;

        int selected = skinManager.CurrentIndex;
        int max = Mathf.Max(0, previewItems.Count - 1);
        int next = Mathf.Min(selected + 1, max);

        skinManager.SelectIndex(next);
        Refresh();
    }

    public void Previous()
    {
        if (skinManager == null) return;

        int selected = skinManager.CurrentIndex;
        int prev = Mathf.Max(selected - 1, 0);

        skinManager.SelectIndex(prev);
        Refresh();
    }

    public void Refresh()
    {
        EnsurePreviewContainerEnabled();

        if (skinManager == null || previewItems.Count == 0)
        {
            Debug.LogWarning("[SkinCarousel] Refresh called but SkinManager is null or no preview items exist.");
            return;
        }

        int selected = Mathf.Clamp(skinManager.CurrentIndex, 0, Mathf.Max(0, previewItems.Count - 1));

        for (int i = 0; i < previewItems.Count; i++)
        {
            Transform tr = previewItems[i];
            if (tr == null) continue;

            bool shouldShow = (i == selected) || (i == selected - 1) || (i == selected + 1);

            if (tr.gameObject.activeSelf != shouldShow)
                tr.gameObject.SetActive(shouldShow);

            if (!shouldShow) continue;

            Vector3 targetLocalPos;
            Vector3 targetScale;
            Color targetTint;

            if (i == selected)
            {
                targetLocalPos = ResolveTargetLocalPosition(centerTransform, centerLocalPosition);
                targetScale = centerScale;
                targetTint = centerTint;
            }
            else if (i == selected - 1)
            {
                targetLocalPos = ResolveTargetLocalPosition(leftTransform, leftLocalPosition);
                targetScale = sideScale;
                targetTint = sideTint;
            }
            else
            {
                targetLocalPos = ResolveTargetLocalPosition(rightTransform, rightLocalPosition);
                targetScale = sideScale;
                targetTint = sideTint;
            }

            // Apply per-skin menu scale if available
            float customMenuScale = 1.0f;
            RocketSkin skin = skinManager.GetSkinAt(i);
            if (skin != null)
            {
                // Ensure we don't scale to 0 if the user forgot to set it (older assets might be 0 if not re-serialized)
                // However, since we defaulted to 1.0f in code, new assets will be 1.0f. 
                // Existing assets might load as 0 until inspected? 
                // Let's protect against 0 just in case, unless 0 is desired.
                if (skin.menuScale > 0.001f)
                    customMenuScale = skin.menuScale;
            }

            tr.localPosition = targetLocalPos;
            tr.localScale = Vector3.Scale(baseItemScale, targetScale) * customMenuScale;
            ApplyTint(tr, targetTint);
        }
    }

    private Vector3 ResolveTargetLocalPosition(Transform anchorWorld, Vector3 fallbackLocal)
    {
        if (previewItemContainer == null) return fallbackLocal;

        // If anchor is provided, convert its world position into the preview container's local space
        if (anchorWorld != null)
            return previewItemContainer.InverseTransformPoint(anchorWorld.position);

        return fallbackLocal;
    }

    private void ApplyTint(Transform tr, Color tint)
    {
        var srs = tr.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in srs) sr.color = tint;

        var img = tr.GetComponent<Image>();
        if (img != null) img.color = tint;
    }

    private void OnSkinChanged(RocketSkin _)
    {
        // If selection changes while menu is open, reposition immediately.
        Refresh();
    }

    // Debug helpers (optional)
    public int PreviewCount => previewItems.Count;
    public Transform GetPreviewAt(int index) => (index >= 0 && index < previewItems.Count) ? previewItems[index] : null;
}
