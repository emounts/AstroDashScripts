using System.Collections.Generic;
using UnityEngine;

// Scalable skin selector UI driver.
public class SkinCarousel : MonoBehaviour
{
    private static readonly HashSet<SkinCarousel> ActiveCarousels = new HashSet<SkinCarousel>();
    public static bool SelectionOpen => ActiveCarousels.Count > 0;

    public static bool AnyOpen() => SelectionOpen;

    [Header("Dependencies")]
    [SerializeField] private SkinManager skinManager;

    [Header("Preview Objects (Optional)")]
    [SerializeField] private List<Transform> previewItems = new List<Transform>();

    [Header("Layout")]
    [SerializeField] private Vector3 downLeft = new Vector3(-1.9f, -0.5f, 0f);
    [SerializeField] private Vector3 upMiddle = new Vector3(0f, 0f, 0f);
    [SerializeField] private Vector3 downRight = new Vector3(1.9f, -0.5f, 0f);
    [SerializeField] private Vector3 outOfView = new Vector3(20f, 0f, 0f);

    [Header("Optional: hide gameplay rocket while selecting")]
    [SerializeField] private List<GameObject> rocketsToHide = new List<GameObject>();
    private readonly List<GameObject> _hiddenDuringCarousel = new List<GameObject>();
    private readonly List<(Rigidbody2D body, bool prevSimulated, Vector2 prevVelocity)> _hiddenBodies = new List<(Rigidbody2D, bool, Vector2)>();

    [Header("Optional: disable start controls while selecting")]
    [SerializeField] private List<GameObject> startObjectsToDisable = new List<GameObject>();
    private readonly List<GameObject> _disabledStartObjects = new List<GameObject>();

    [Header("Optional: pause movement on preview rockets")]
    [SerializeField] private bool freezePreviewMovement = true;
    private readonly List<(RocketController controller, bool prevEnabled)> _frozenControllers = new List<(RocketController, bool)>();
    private readonly List<(Rigidbody2D body, bool prevSimulated, Vector2 prevVelocity)> _frozenBodies = new List<(Rigidbody2D, bool, Vector2)>();

    private void Awake()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
    }

    private void OnEnable()
    {
        ActiveCarousels.Add(this);
        HideGameplayRockets();
        DisableStartObjects();
        if (freezePreviewMovement)
            FreezePreviewControllers();
        GameEvents.SkinChanged += OnSkinChanged;
        Refresh();
    }

    private void OnDisable()
    {
        ActiveCarousels.Remove(this);
        GameEvents.SkinChanged -= OnSkinChanged;
        ShowGameplayRockets();
        EnableStartObjects();
        if (freezePreviewMovement)
            RestorePreviewControllers();
    }

    private void Update()
    {
        // Extra safety: if the carousel stays open and some other script reactivates rockets, re-hide/re-freeze.
        if (SelectionOpen)
        {
            HideGameplayRockets();
            if (freezePreviewMovement)
                FreezePreviewControllers();
        }
    }

    private void Start()
    {
        Refresh();
    }

    public void Next()
    {
        if (skinManager != null)
        {
            skinManager.Next();
        }
        else
        {
            int selected = PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);
            int max = previewItems != null && previewItems.Count > 0 ? previewItems.Count - 1 : 0;
            selected = Mathf.Min(selected + 1, max);
            PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, selected);
            GameEvents.RaiseSkinChanged(null);
        }
        Refresh();
    }

    public void Previous()
    {
        if (skinManager != null)
        {
            skinManager.Previous();
        }
        else
        {
            int selected = PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);
            selected = Mathf.Max(selected - 1, 0);
            PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, selected);
            GameEvents.RaiseSkinChanged(null);
        }
        Refresh();
    }

    public void Refresh()
    {
        if (previewItems == null || previewItems.Count == 0) return;

        // If the carousel stays enabled across menu toggles, ensure we still hide/freeze things.
        if (SelectionOpen && _hiddenDuringCarousel.Count == 0)
            HideGameplayRockets();
        if (freezePreviewMovement && SelectionOpen && _frozenControllers.Count == 0 && _frozenBodies.Count == 0)
            FreezePreviewControllers();

        int selected = skinManager != null ? skinManager.CurrentIndex : PlayerPrefs.GetInt(GameConstants.PrefSkinMenuPosition, 0);
        int clamped = Mathf.Clamp(selected, 0, previewItems.Count - 1);

        // Keep selection consistent with what we can actually preview.
        if (skinManager != null && clamped != skinManager.CurrentIndex)
        {
            skinManager.SelectIndex(clamped);
            selected = skinManager.CurrentIndex;
        }
        else
        {
            selected = clamped;
            if (skinManager == null)
                PlayerPrefs.SetInt(GameConstants.PrefSkinMenuPosition, selected);
        }

        for (int i = 0; i < previewItems.Count; i++)
        {
            Transform tr = previewItems[i];
            if (tr == null) continue;

            Vector3 target;
            if (i == selected)
                target = upMiddle;
            else if (i == selected - 1)
                target = downLeft;
            else if (i == selected + 1)
                target = downRight;
            else
                target = outOfView;

            RectTransform rt = tr as RectTransform;
            if (rt != null)
                rt.anchoredPosition3D = target;
            else
                tr.localPosition = target;

            // Ensure all preview items are visible while selecting skins.
            if (!tr.gameObject.activeSelf)
                tr.gameObject.SetActive(true);
        }
    }

    public void RegisterPreviewItem(Transform item)
    {
        if (item == null) return;
        if (!previewItems.Contains(item)) previewItems.Add(item);
    }

    public int PreviewCount => previewItems != null ? previewItems.Count : 0;
    public Transform GetPreviewAt(int index)
    {
        if (previewItems == null) return null;
        if (index < 0 || index >= previewItems.Count) return null;
        return previewItems[index];
    }

    private void HideGameplayRockets()
    {
        _hiddenDuringCarousel.Clear();
        _hiddenBodies.Clear();

        // If SkinManager has a current rocket, hide it first.
        if (skinManager != null && skinManager.CurrentRocket != null && skinManager.CurrentRocket.activeSelf)
        {
            GameObject go = skinManager.CurrentRocket;
            HideAndStore(go);
        }

        if (rocketsToHide != null && rocketsToHide.Count > 0)
        {
            for (int i = 0; i < rocketsToHide.Count; i++)
            {
                GameObject go = rocketsToHide[i];
                if (go == null) continue;
                HideAndStore(go);
            }
            return;
        }

        // Fallback: hide any active rocket tagged RocketShip in the pregame UI.
        GameObject[] rockets = GameObject.FindGameObjectsWithTag("RocketShip");
        for (int i = 0; i < rockets.Length; i++)
        {
            GameObject go = rockets[i];
            if (go == null) continue;
            HideAndStore(go);
        }
    }

    private void ShowGameplayRockets()
    {
        for (int i = 0; i < _hiddenDuringCarousel.Count; i++)
        {
            GameObject go = _hiddenDuringCarousel[i];
            if (go == null) continue;
            if (!go.activeSelf)
                go.SetActive(true);
        }
        _hiddenDuringCarousel.Clear();

        for (int i = 0; i < _hiddenBodies.Count; i++)
        {
            var pair = _hiddenBodies[i];
            if (pair.body == null) continue;
            pair.body.simulated = pair.prevSimulated;
            pair.body.linearVelocity = pair.prevVelocity;
        }
        _hiddenBodies.Clear();
    }

    private void DisableStartObjects()
    {
        _disabledStartObjects.Clear();
        if (startObjectsToDisable == null) return;

        for (int i = 0; i < startObjectsToDisable.Count; i++)
        {
            GameObject go = startObjectsToDisable[i];
            if (go == null) continue;
            if (go.activeSelf)
            {
                go.SetActive(false);
                _disabledStartObjects.Add(go);
            }
        }
    }

    private void EnableStartObjects()
    {
        for (int i = 0; i < _disabledStartObjects.Count; i++)
        {
            GameObject go = _disabledStartObjects[i];
            if (go == null) continue;
            if (!go.activeSelf)
                go.SetActive(true);
        }
        _disabledStartObjects.Clear();
    }

    private void FreezePreviewControllers()
    {
        _frozenControllers.Clear();
        _frozenBodies.Clear();
        if (previewItems == null) return;

        for (int i = 0; i < previewItems.Count; i++)
        {
            Transform tr = previewItems[i];
            if (tr == null) continue;

            RocketController rc = tr.GetComponentInChildren<RocketController>(includeInactive: true);
            if (rc == null) continue;

            bool wasEnabled = rc.enableMovement;
            _frozenControllers.Add((rc, wasEnabled));
            rc.enableMovement = false;
            if (rc.rb != null)
            {
                Rigidbody2D body = rc.rb;
                _frozenBodies.Add((body, body.simulated, body.linearVelocity));
                body.simulated = false;
                body.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnSkinChanged(RocketSkin _)
    {
        if (!SelectionOpen) return;
        HideGameplayRockets();
        if (freezePreviewMovement)
            FreezePreviewControllers();
    }

    private void HideAndStore(GameObject go)
    {
        if (go == null) return;
        if (!_hiddenDuringCarousel.Contains(go))
            _hiddenDuringCarousel.Add(go);

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null && !_hiddenBodies.Exists(x => x.body == rb))
        {
            _hiddenBodies.Add((rb, rb.simulated, rb.linearVelocity));
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }

        if (go.activeSelf)
            go.SetActive(false);
    }

    private void RestorePreviewControllers()
    {
        for (int i = 0; i < _frozenControllers.Count; i++)
        {
            var pair = _frozenControllers[i];
            if (pair.controller == null) continue;
            pair.controller.enableMovement = pair.prevEnabled;
        }
        _frozenControllers.Clear();

        for (int i = 0; i < _frozenBodies.Count; i++)
        {
            var pair = _frozenBodies[i];
            if (pair.body == null) continue;
            pair.body.simulated = pair.prevSimulated;
            pair.body.linearVelocity = pair.prevVelocity;
        }
        _frozenBodies.Clear();
    }
}
