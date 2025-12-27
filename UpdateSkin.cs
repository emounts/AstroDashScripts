using UnityEngine;

public class UpdateSkin : MonoBehaviour
{
    public GameObject ActivateSkin1;
    public GameObject ActivateSkin2;
    public GameObject ActivateSkin3;
    public GameObject ActivateSkin4;
    public GameObject ActivateSkin5;

    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    private void OnEnable()
    {
        GameEvents.SkinChanged += OnSkinChanged;
        ApplyCurrentSelection();
    }

    private void OnDisable()
    {
        GameEvents.SkinChanged -= OnSkinChanged;
    }

    private void OnSkinChanged(RocketSkin _)
    {
        ApplyCurrentSelection();
    }

    private void ApplyCurrentSelection()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
        int selected = skinManager != null ? skinManager.CurrentIndex : PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);

        SetActiveSafe(ActivateSkin1, selected == 0);
        SetActiveSafe(ActivateSkin2, selected == 1);
        SetActiveSafe(ActivateSkin3, selected == 2);
        SetActiveSafe(ActivateSkin4, selected == 3);
        SetActiveSafe(ActivateSkin5, selected == 4);
    }

    private static void SetActiveSafe(GameObject go, bool active)
    {
        if (go == null) return;
        if (go.activeSelf != active) go.SetActive(active);
    }
}
