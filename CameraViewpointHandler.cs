using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraViewpointHandler : MonoBehaviour
{
    public enum Constraint { Landscape, Portrait }

    public Color wireColor = Color.white;
    public float UnitsSize = 1f;
    public Constraint constraint = Constraint.Portrait;
    public static CameraViewpointHandler Instance;
    public new Camera camera;

    public bool executeInUpdate = true;

    private float _width;
    private float _height;

    private Vector3 _bl, _bc, _br;
    private Vector3 _ml, _mc, _mr;
    private Vector3 _tl, _tc, _tr;

    public float Width => _width;
    public float Height => _height;

    public Vector3 BottomLeft => _bl;
    public Vector3 BottomCenter => _bc;
    public Vector3 BottomRight => _br;
    public Vector3 MiddleLeft => _ml;
    public Vector3 MiddleCenter => _mc;
    public Vector3 MiddleRight => _mr;
    public Vector3 TopLeft => _tl;
    public Vector3 TopCenter => _tc;
    public Vector3 TopRight => _tr;

    private void Awake()
    {
        camera = GetComponent<Camera>();
        Instance = this;
        ComputeResolution();
    }

    // IMPORTANT: LateUpdate, not Update
    private void LateUpdate()
    {
        if (executeInUpdate)
            ComputeResolution();
    }

    private void ComputeResolution()
    {
        float leftX, rightX, topY, bottomY;

        if (constraint == Constraint.Landscape)
            camera.orthographicSize = 1f / camera.aspect * UnitsSize / 2f;
        else
            camera.orthographicSize = UnitsSize / 2f;

        _height = 2f * camera.orthographicSize;
        _width = _height * camera.aspect;

        float cameraX = camera.transform.position.x;
        float cameraY = camera.transform.position.y;

        leftX = cameraX - _width / 2f;
        rightX = cameraX + _width / 2f;
        topY = cameraY + _height / 2f;
        bottomY = cameraY - _height / 2f;

        // Keep your Z behavior as-is (0). If your HUD plane isn't Z=0, tell me and we’ll fix that too.
        const float z = 0f;

        _bl = new Vector3(leftX, bottomY, z);
        _bc = new Vector3(cameraX, bottomY, z);
        _br = new Vector3(rightX, bottomY, z);

        _ml = new Vector3(leftX, cameraY, z);
        _mc = new Vector3(cameraX, cameraY, z);
        _mr = new Vector3(rightX, cameraY, z);

        _tl = new Vector3(leftX, topY, z);
        _tc = new Vector3(cameraX, topY, z);
        _tr = new Vector3(rightX, topY, z);
    }
}
