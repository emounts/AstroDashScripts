using UnityEngine;

public class NextAndBackButtons : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private SkinCarousel carousel;

    [Header("Preview Root (optional)")]
    public Transform PreviewRoot;

    [Header("Legacy preview objects (optional, kept for inspector compatibility)")]
    public GameObject Ship01;
    public GameObject Ship02;
    public GameObject Ship03;
    public GameObject Ship04;
    public GameObject Ship05;

    [Header("Legacy menus (optional)")]
    public GameObject MenuToActivate;
    public GameObject MenuToDeactivate;

    [Header("Legacy selection markers (optional)")]
    public GameObject ActivateSkin1;
    public GameObject ActivateSkin2;
    public GameObject ActivateSkin3;
    public GameObject ActivateSkin4;
    public GameObject ActivateSkin5;

    private void Awake()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
    }

    private void Start()
    {
        if (carousel == null) carousel = GetComponent<SkinCarousel>();
        if (carousel == null) carousel = gameObject.AddComponent<SkinCarousel>();

        RegisterLegacyPreviews();
        carousel.Refresh();

        if (skinManager != null)
            PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, skinManager.CurrentIndex);

        UpdateLegacySelectionMarkers();
    }

        private void RegisterLegacyPreviews()
    {
        if (carousel == null) return;

        // Preferred scalable path: register all children under PreviewRoot (allows 6th/7th skin easily).
        if (PreviewRoot != null)
        {
            int childCount = PreviewRoot.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = PreviewRoot.GetChild(i);
                if (child == null) continue;
                carousel.RegisterPreviewItem(child);
            }
            return;
        }

        // Legacy fixed references
        if (Ship01 != null) carousel.RegisterPreviewItem(Ship01.transform);
        if (Ship02 != null) carousel.RegisterPreviewItem(Ship02.transform);
        if (Ship03 != null) carousel.RegisterPreviewItem(Ship03.transform);
        if (Ship04 != null) carousel.RegisterPreviewItem(Ship04.transform);
        if (Ship05 != null) carousel.RegisterPreviewItem(Ship05.transform);
    }

    public void NextOption()
    {
        if (carousel != null) carousel.Next();
        else if (skinManager != null) skinManager.Next();
        UpdateLegacySelectionMarkers();
    }

    public void BackOption()
    {
        if (carousel != null) carousel.Previous();
        else if (skinManager != null) skinManager.Previous();
        UpdateLegacySelectionMarkers();
    }

    public void SelectedShip()
    {
        if (IsCurrentSelectionLocked())
            return;

        if (MenuToActivate != null) MenuToActivate.SetActive(true);
        if (MenuToDeactivate != null) MenuToDeactivate.SetActive(false);
        UpdateLegacySelectionMarkers();
    }

    private void UpdateLegacySelectionMarkers()
    {
        int index = skinManager != null ? skinManager.CurrentIndex : PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);
        SetActiveSafe(ActivateSkin1, index == 0);
        SetActiveSafe(ActivateSkin2, index == 1);
        SetActiveSafe(ActivateSkin3, index == 2);
        SetActiveSafe(ActivateSkin4, index == 3);
        SetActiveSafe(ActivateSkin5, index == 4);
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf != active) go.SetActive(active);
    }

    private bool IsCurrentSelectionLocked()
    {
        int index = skinManager != null ? skinManager.CurrentIndex : PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);

        // Data-driven lock check via SkinManager + RocketSkin.
        if (skinManager != null && !skinManager.IsUnlocked(index))
            return true;

        // Legacy: respect tag == "Locked" on the currently selected preview object.
        Transform currentPreview = carousel != null && carouselPreviewCount() > index
            ? getCarouselPreview(index)
            : getLegacyPreviewByIndex(index);

        if (currentPreview != null && currentPreview.CompareTag("Locked"))
            return true;

        return false;
    }

    private int carouselPreviewCount()
    {
        return carousel != null ? carousel.PreviewCount : 0;
    }

    private Transform getCarouselPreview(int index)
    {
        return carousel != null ? carousel.GetPreviewAt(index) : null;
    }

    private Transform getLegacyPreviewByIndex(int index)
    {
        switch (index)
        {
            case 0: return Ship01 != null ? Ship01.transform : null;
            case 1: return Ship02 != null ? Ship02.transform : null;
            case 2: return Ship03 != null ? Ship03.transform : null;
            case 3: return Ship04 != null ? Ship04.transform : null;
            case 4: return Ship05 != null ? Ship05.transform : null;
            default: return null;
        }
    }
}
