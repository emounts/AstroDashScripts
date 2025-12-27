using UnityEngine;

public class MoonUp : MonoBehaviour
{
    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    public Transform moonT;
    public GameObject moonGO;
    public Rigidbody2D moonRB;

    // Legacy fields (kept for inspector compatibility)
    public Transform rs1;
    public Transform rs2;
    public Transform rs3;
    public Transform rs4;
    public Transform rs5;
    public GameObject RS1;
    public GameObject RS2;
    public GameObject RS3;
    public GameObject RS4;
    public GameObject RS5;

    private float rocketSpeed;
    public float slowingSpeed = 0.5f;

    private Vector3 _lastPos;
    private bool _hasLastPos;
    private CelestialObjectController _celestialController;

    private void Awake()
    {
        // If a CelestialObjectController is present on this object, let it drive motion entirely.
        _celestialController = GetComponent<CelestialObjectController>();
        if (_celestialController != null)
        {
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (!enabled) return;
        if (skinManager == null) skinManager = SkinManager.Instance;

        // Preferred: use active rocket rigidbody if available.
        Rigidbody2D currentRb = skinManager != null ? skinManager.CurrentRocketRigidbody : null;
        if (currentRb != null)
        {
            rocketSpeed = currentRb.linearVelocity.magnitude;
            if (moonRB != null)
                moonRB.linearVelocity = Mathf.Max(rocketSpeed - slowingSpeed, 0f) * Vector2.up;
            return;
        }

        // Legacy fallback: compute speed from the currently active rocket transform.
        Transform active = GetActiveRocketTransform();
        if (active == null || moonRB == null)
        {
            if (moonRB != null) moonRB.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 pos = active.position;
        if (_hasLastPos)
            rocketSpeed = (pos - _lastPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);

        _lastPos = pos;
        _hasLastPos = true;

        moonRB.linearVelocity = Mathf.Max(rocketSpeed - slowingSpeed, 0f) * Vector2.up;
    }

    private Transform GetActiveRocketTransform()
    {
        if (RS1 != null && RS1.activeSelf) return rs1;
        if (RS2 != null && RS2.activeSelf) return rs2;
        if (RS3 != null && RS3.activeSelf) return rs3;
        if (RS4 != null && RS4.activeSelf) return rs4;
        if (RS5 != null && RS5.activeSelf) return rs5;
        return null;
    }
}
