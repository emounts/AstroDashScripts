using UnityEngine;

public class EarthUp : MonoBehaviour
{
    public Rigidbody2D rb;
    public float upSpeed = 80f;
    public float destroyDelay = 1.5f;
    [Tooltip("If true, large legacy speeds are scaled down to per-second units automatically.")]
    public bool autoScaleLegacySpeed = true;
    public float legacySpeedThreshold = 20f;
    public float legacySpeedScale = 1f / 60f;
    private bool _legacySpeedAdjusted;

    void FixedUpdate()
    {
        if (rb == null) return;

        MaybeConvertLegacySpeed();
        rb.linearVelocity = upSpeed * Vector2.up;

        if (gameObject.tag == "Earth")
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    private void MaybeConvertLegacySpeed()
    {
        if (!autoScaleLegacySpeed) return;
        if (_legacySpeedAdjusted) return;
        if (upSpeed <= legacySpeedThreshold) return;

        upSpeed *= legacySpeedScale;
        _legacySpeedAdjusted = true;
    }
}
