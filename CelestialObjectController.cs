using UnityEngine;

/// <summary>
/// Positions a celestial object in view, giving it a parallax effect relative to the rocket's movement.
/// The object's vertical speed is a factor of the rocket's speed.
/// The object's horizontal position tracks the rocket's X-position based on a scaling factor.
/// </summary>
public class CelestialObjectController : MonoBehaviour
{
    [Header("Placement")]
    [Tooltip("Initial horizontal position as a viewport percentage (0=left, 50=center, 100=right).")]
    [Range(0f, 100f)] public float xPosition = 50f;

    [Header("Timing")]
    [Tooltip("Seconds of rocket travel before object becomes visible.")]
    public float timeAppear = 0f;

    [Header("Parallax / Tracking")]
    [Tooltip("Controls how much the object tracks the rocket's X-position.\n0 = Perfectly stationary in the world.\n100 = Tracks the rocket's X-position perfectly.")]
    [Range(0f, 100f)] public float sizeScaling = 0f;
    
    [Tooltip("Multiplier for the rocket's upward speed. \n0 = Stationary vertically.\n1 = Moves at the same speed as the rocket.")]
    [Range(0f, 1f)] public float relativeSpeedFactor = 0.7f;
    
    [Tooltip("World-space Y offset above the rocket to spawn when appearing.")]
    public float spawnYOffset = 10f;

    [Tooltip("How slowly the object drifts down relative to the camera after the player has crashed.")]
    public float postDeathDriftSpeed = 0.1f;

    [Header("Debug")]
    [Tooltip("Runtime upward speed applied to this object.")]
    public float debugCurrentSpeed;
    [Tooltip("Last sampled rocket upward speed.")]
    public float debugRocketSpeed;

    [Header("References (optional)")]
    [SerializeField] private SkinManager skinManager;
    [SerializeField] private Camera targetCamera;

    private Renderer[] _renderers;
    private bool _isStarted;
    private bool _isVisible;
    private Transform _rocket;
    private Rigidbody2D _body;
    
    private float _appearStartTime;
    private float _enableTime;
    
    // The world-space X-coordinate that represents a stationary (sizeScaling=0) position.
    private float _worldAnchorX;
    private bool _hasBeenPlaced;
    private bool _isPlayerDead;
    private Vector3 _lastCameraPos;

    private void Awake()
    {
        // Resolve references
        if (skinManager == null) skinManager = SkinManager.Instance;
        if (targetCamera == null) targetCamera = Camera.main;
        _renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        _body = GetComponent<Rigidbody2D>();

        // Set initial state
        SetVisible(false);
        _enableTime = Time.time;
        
        // Ensure the object starts offscreen
        InitialPlacement();
    }

    private void OnEnable()
    {
        // Reset state for object pooling
        _rocket = ResolveRocket();
        _hasBeenPlaced = false;
        _isStarted = false;
        _isPlayerDead = false;
        _enableTime = Time.time;
        SetVisible(false);

        GameEvents.PlayerCrashed += OnPlayerCrashed;
    }

    private void OnDisable()
    {
        GameEvents.PlayerCrashed -= OnPlayerCrashed;
    }

    private void OnPlayerCrashed()
    {
        _isPlayerDead = true;
        if (targetCamera != null)
        {
            _lastCameraPos = targetCamera.transform.position;
        }
    }


    private void Update()
    {
        if (_rocket == null) _rocket = ResolveRocket();
        if (targetCamera == null) targetCamera = Camera.main;

        if (_rocket == null || targetCamera == null)
        {
            return; // Cannot proceed without rocket or camera
        }

        // Place the object once the rocket is available
        if (!_hasBeenPlaced)
        {
            InitialPlacement();
        }

        // Determine if the object should be visible based on timing
        HandleVisibility();
        
        // After becoming visible, check if it has moved offscreen to be destroyed
        if (_isVisible)
        {
            CheckAndDestroyIfOffscreen();
        }
    }

    private void FixedUpdate()
    {
        // The death state takes priority over all other logic.
        if (_isPlayerDead)
        {
            if (targetCamera == null) return;

            // Calculate camera's vertical velocity since the last physics frame.
            float cameraVy = (targetCamera.transform.position.y - _lastCameraPos.y) / Time.fixedDeltaTime;
            _lastCameraPos = targetCamera.transform.position;
            
            // The object's target speed is the camera's speed, minus the relative drift.
            float targetVy = cameraVy - postDeathDriftSpeed;

            if (_body != null)
            {
                // When dead, disable gravity and set velocity for a predictable drift.
                _body.gravityScale = 0;
                _body.linearVelocity = new Vector2(0, targetVy);
            }
            else
            {
                transform.position += new Vector3(0, targetVy * Time.fixedDeltaTime, 0);
            }
            return;
        }

        // Physics-based movement should only happen in FixedUpdate and if visible
        if (!_isVisible || _rocket == null)
        {
            if (_body != null) _body.linearVelocity = Vector2.zero;
            return;
        }

        // --- VERTICAL MOVEMENT ---
        float rocketSpeed = SampleRocketUpSpeed();
        debugRocketSpeed = rocketSpeed;
        float objectUpSpeed = rocketSpeed * relativeSpeedFactor;
        debugCurrentSpeed = objectUpSpeed;

        // --- HORIZONTAL MOVEMENT ---
        float trackingFactor = Mathf.Clamp01(sizeScaling / 100f);
        // Linearly interpolate between the fixed world anchor and the rocket's live position.
        float targetX = Mathf.Lerp(_worldAnchorX, _rocket.position.x, trackingFactor);

        // Apply movement
        if (_body != null)
        {
            // This script assumes full control of velocity; disable gravity during active movement.
            _body.gravityScale = 0;
            
            // For rigidbodies, set velocity to move towards the target position.
            // This is smoother and more physics-correct than setting the position directly.
            float requiredVelX = (targetX - _body.position.x) / Time.fixedDeltaTime;
            _body.linearVelocity = new Vector2(requiredVelX, objectUpSpeed);
        }
        else
        {
            // For non-rigidbodies, update position directly.
            transform.position += new Vector3(targetX - transform.position.x, objectUpSpeed * Time.fixedDeltaTime, 0);
        }
    }

    /// <summary>
    /// Places the object at its starting position and establishes its stationary world anchor.
    /// </summary>
    private void InitialPlacement()
    {
        _rocket = ResolveRocket();
        if (_rocket == null || targetCamera == null) return;

        // Determine the horizontal position based on the viewport percentage.
        float viewportX = Mathf.Clamp01(xPosition / 100f);
        // We use the camera's Z distance to project the viewport coordinate into the world.
        float distance = transform.position.z - targetCamera.transform.position.z;
        Vector3 worldPos = targetCamera.ViewportToWorldPoint(new Vector3(viewportX, 0.5f, distance));

        // The stationary anchor is this initial world X-position.
        _worldAnchorX = worldPos.x;

        // Position the object high above the rocket, just out of view.
        float camHalfHeight = targetCamera.orthographic ? targetCamera.orthographicSize : 5f;
        float objHalfHeight = _renderers.Length > 0 ? _renderers[0].bounds.extents.y : 0.5f;
        float spawnY = _rocket.position.y + camHalfHeight + objHalfHeight + spawnYOffset;

        transform.position = new Vector3(_worldAnchorX, spawnY, 0f);
        _hasBeenPlaced = true;
    }

    /// <summary>
    /// Checks rocket travel time to determine if the object should become visible.
    /// </summary>
    private void HandleVisibility()
    {
        // Detect when the rocket starts traveling.
        if (!_isStarted)
        {
            Rigidbody2D rocketBody = _rocket.GetComponent<Rigidbody2D>();
            float speed = rocketBody != null ? rocketBody.linearVelocity.magnitude : 0f;
            if (speed > 0.1f || Time.time - _enableTime > 1f)
            {
                _isStarted = true;
                _appearStartTime = Time.time;
            }
        }

        float travelElapsed = _isStarted ? Time.time - _appearStartTime : 0f;
        bool shouldBeVisible = _hasBeenPlaced && (timeAppear <= 0f || travelElapsed >= timeAppear);

        if (shouldBeVisible != _isVisible)
        {
            SetVisible(shouldBeVisible);
        }
    }
    
    private void SetVisible(bool visible)
    {
        _isVisible = visible;
        if (_renderers == null) return;
        foreach (var r in _renderers)
        {
            if (r != null) r.enabled = visible;
        }

        if (_body != null)
        {
            _body.simulated = visible;
        }
    }

    private Transform ResolveRocket()
    {
        if (skinManager != null && skinManager.CurrentRocketTransform != null)
            return skinManager.CurrentRocketTransform;

        GameObject tagged = GameObject.FindGameObjectWithTag("RocketShip");
        return tagged != null ? tagged.transform : null;
    }

    private float SampleRocketUpSpeed()
    {
        if (_rocket == null) return GameConstants.RocketInitialSpeed;
        Rigidbody2D rb = _rocket.GetComponent<Rigidbody2D>();
        float vy = rb != null ? rb.linearVelocity.y : 0f;
        if (vy <= 0.01f)
            vy = GameConstants.RocketInitialSpeed;
        return vy;
    }
    
    private void CheckAndDestroyIfOffscreen()
    {
        if (targetCamera == null) return;

        float halfHeightVp = GetObjectBoundsViewport().height / 2f;
        Vector3 vp = targetCamera.WorldToViewportPoint(transform.position);

        // Check if object is fully below the screen
        if (vp.y < 0f - halfHeightVp)
        {
            Destroy(gameObject);
        }
    }
    
    private Rect GetObjectBoundsViewport()
    {
        if (_renderers.Length == 0) return new Rect(0,0,0,0);

        Bounds b = _renderers[0].bounds;
        for (int i = 1; i < _renderers.Length; i++)
        {
            b.Encapsulate(_renderers[i].bounds);
        }

        Vector3 min = targetCamera.WorldToViewportPoint(b.min);
        Vector3 max = targetCamera.WorldToViewportPoint(b.max);
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }
}