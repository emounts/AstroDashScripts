using UnityEngine;

[ExecuteAlways]
public class AnchorGameObject : MonoBehaviour
{
    public enum AnchorType
    {
        BottomLeft,
        BottomCenter,
        BottomRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        TopLeft,
        TopCenter,
        TopRight,
    }

    [Tooltip("Update anchor every frame. Turn this on for gameplay.")]
    public bool executeInUpdate = true;

    public AnchorType anchorType;
    public Vector3 anchorOffset;

    [Tooltip("How close is 'close enough' before we stop moving (prevents micro-jitter).")]
    public float positionEpsilon = 0.0001f;

    void OnEnable()
    {
        // Do one placement immediately
        ApplyAnchor();
    }

    // Use LateUpdate so this runs AFTER the camera has finished moving for the frame.
    void LateUpdate()
    {
        if (executeInUpdate)
            ApplyAnchor();
    }

    void ApplyAnchor()
    {
        var handler = CameraViewpointHandler.Instance;
        if (handler == null) return;

        Vector3 anchor = anchorType switch
        {
            AnchorType.BottomLeft => handler.BottomLeft,
            AnchorType.BottomCenter => handler.BottomCenter,
            AnchorType.BottomRight => handler.BottomRight,
            AnchorType.MiddleLeft => handler.MiddleLeft,
            AnchorType.MiddleCenter => handler.MiddleCenter,
            AnchorType.MiddleRight => handler.MiddleRight,
            AnchorType.TopLeft => handler.TopLeft,
            AnchorType.TopCenter => handler.TopCenter,
            _ => handler.TopRight,
        };

        Vector3 target = anchor + anchorOffset;

        // Avoid Equals. Use a tolerance.
        if ((transform.position - target).sqrMagnitude > positionEpsilon * positionEpsilon)
            transform.position = target;
    }
}
