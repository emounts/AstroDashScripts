using UnityEngine;

public class MainMenuRocketPreview : MonoBehaviour
{
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private Transform anchor;          // where the rocket should appear in main menu
    [SerializeField] private GameObject skinsMenuRoot;  // the menu panel that opens/closes

    private GameObject currentPreview;

    private void Awake()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
    }

    private void OnEnable()
    {
        GameEvents.SkinChanged += OnSkinChanged;
        SpawnOrRefresh();
    }

    private void OnDisable()
    {
        GameEvents.SkinChanged -= OnSkinChanged;
        DestroyPreview();
    }

    private void Update()
    {
        // If the skins menu is open, hide main menu preview
        bool menuOpen = skinsMenuRoot != null && skinsMenuRoot.activeInHierarchy;
        if (menuOpen)
        {
            if (currentPreview != null) currentPreview.SetActive(false);
        }
        else
        {
            if (currentPreview == null) SpawnOrRefresh();
            if (currentPreview != null) currentPreview.SetActive(true);
        }
    }

    private void OnSkinChanged(RocketSkin _)
    {
        SpawnOrRefresh();
    }

    private void SpawnOrRefresh()
    {
        if (skinManager == null) return;
        RocketSkin skin = skinManager.CurrentSkin;
        if (skin == null || skin.rocketPrefab == null) return;

        DestroyPreview();

        Vector3 pos = anchor != null ? anchor.position : transform.position;
        Quaternion rot = anchor != null ? anchor.rotation : transform.rotation;

        currentPreview = Instantiate(skin.rocketPrefab, pos, rot);
        currentPreview.name = "MainMenuRocketPreview(Clone)";

        // Disable gameplay movement/physics on the menu preview, keep animation
        var rb2d = currentPreview.GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.simulated = false;

        var rc = currentPreview.GetComponent<RocketController>();
        if (rc != null) { rc.enableMovement = false; rc.enabled = false; }

        var mv1 = currentPreview.GetComponent<RocketMovement1>();
        if (mv1 != null) mv1.enabled = false;

        var mvMenu = currentPreview.GetComponent<RocketMovementMenu>();
        if (mvMenu != null) mvMenu.enabled = false;

        var cols2d = currentPreview.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in cols2d) c.enabled = false;

        var anim = currentPreview.GetComponent<Animator>();
        if (anim != null) anim.enabled = true;
    }

    private void DestroyPreview()
    {
        if (currentPreview != null) Destroy(currentPreview);
        currentPreview = null;
    }
}
