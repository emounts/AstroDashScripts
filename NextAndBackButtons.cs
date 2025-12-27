using UnityEngine;

public class NextAndBackButtons : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private SkinCarousel carousel;

    [Header("Menus")]
    public GameObject MenuToActivate;
    public GameObject MenuToDeactivate;

    private void Awake()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
    }

    private void Start()
    {
        if (carousel == null) carousel = GetComponent<SkinCarousel>();
        if (carousel != null) carousel.Refresh();

        if (skinManager != null)
            PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, skinManager.CurrentIndex);
    }

    public void NextOption()
    {
        if (carousel != null) carousel.Next();
        else if (skinManager != null) skinManager.Next();
    }

    public void BackOption()
    {
        if (carousel != null) carousel.Previous();
        else if (skinManager != null) skinManager.Previous();
    }

    public void SelectedShip()
{
    int idx = SkinManager.Instance != null
        ? SkinManager.Instance.CurrentIndex
        : PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);

    PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, idx);
    PlayerPrefs.Save(); // important

    if (SkinManager.Instance != null)
        SkinManager.Instance.NotifySkinChanged();

    if (MenuToActivate != null) MenuToActivate.SetActive(true);
    if (MenuToDeactivate != null) MenuToDeactivate.SetActive(false);
}

}
