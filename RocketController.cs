using UnityEngine;

// Single movement implementation shared by gameplay rockets and menu rockets.
[RequireComponent(typeof(Rigidbody2D))]
public class RocketController : MonoBehaviour
{
    [Header("Runtime (auto-wired if empty)")]
    public Rigidbody2D rb;
    public Transform rocketTransform;
    public Animator animator;

    [Header("Tuning")]
    public float initialSpeed = GameConstants.RocketInitialSpeed;
    public float finalSpeed = GameConstants.RocketFinalSpeed;
    public float maxHeightSpeedIncrease = 800f;
    public float verticalSpeedMultiplier = 2f;
    [Tooltip("If true, legacy serialized speeds (e.g. 50, 125, 200) are auto-scaled down to per-second units.")]
    public bool autoScaleLegacySpeeds = true;
    [Tooltip("Speeds above this value will be treated as legacy and scaled by legacySpeedScale.")]
    public float legacySpeedThreshold = 20f;
    [Tooltip("Legacy speeds are multiplied by this value once (default ~1/60s).")]
    public float legacySpeedScale = 1f / 60f;

    [Header("Input")]
    public bool useMouseInput = false;
    public KeyCode keyboardKey = KeyCode.Space;

    [Header("Start Behavior")]
    public bool requireFirstInputToStart = true;
    public bool flyStraightOnStart = true;
    public float startAccelerationDuration = 2.5f;

    [Header("Runtime Toggle")]
    [Tooltip("If false, RocketController will not modify Rigidbody2D velocity.")]
    public bool enableMovement = true;

    [Header("Optional: time-based speed ramp (menu / special modes)")]
    public bool useTimeBasedSpeedIncrease = false;
    public float timeBasedSpeedIncreaseMaxHeight = 670f;
    public float timeBasedSpeedIncreaseIntervalSeconds = 1f;
    public float timeBasedSpeedIncreaseAmount = 0.73f;

    [Header("Movement Direction")]
    public Vector2 direction = Vector2.left;

    private float _speed;
    private float _elapsed;
    private bool _started;
    private bool _legacySpeedsAdjusted;
    private float _startElapsedTime;

    private void Awake()
    {
        ApplyGlobalSpeedConstants();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rocketTransform == null) rocketTransform = transform;
        if (animator == null) animator = GetComponent<Animator>();
        MaybeConvertLegacySpeeds();
    }

    private void OnEnable()
    {
        ApplyGlobalSpeedConstants();
        MaybeConvertLegacySpeeds();
    }

    public void ApplySkinTuning(RocketSkin skin)
    {
        if (skin == null) return;
        // Speeds are enforced globally; ignore per-skin speed overrides.
        initialSpeed = GameConstants.RocketInitialSpeed;
        finalSpeed = GameConstants.RocketFinalSpeed;

        // Only apply skin overrides if they are set (non-zero/valid), otherwise preserve existing defaults.
        if (skin.movement.maxHeightSpeedIncrease > 0)
            maxHeightSpeedIncrease = skin.movement.maxHeightSpeedIncrease;
        
        if (skin.movement.verticalSpeedMultiplier > 0)
            verticalSpeedMultiplier = skin.movement.verticalSpeedMultiplier;
        
        // For booleans, we can't distinguish "false" from "uninitialized", so we take the value.
        // (Assuming false is a safe default for gameplay).
        useMouseInput = skin.movement.useMouseInput;
        
        if (skin.movement.keyboardKey != KeyCode.None)
            keyboardKey = skin.movement.keyboardKey;
            
        _legacySpeedsAdjusted = false;
        MaybeConvertLegacySpeeds();
    }

    private void FixedUpdate()
    {
        if (!enableMovement)
            return;

        bool pressed = useMouseInput ? Input.GetMouseButton(0) : Input.GetKey(keyboardKey);

        // Transition from pre-start to started
        if (requireFirstInputToStart && !_started)
        {
            if (pressed)
            {
                _started = true;
                if (animator != null)
                {
                    animator.SetBool("IsStarted", true);
                }
            }
        }
        
        if (animator != null)
        {
            animator.SetBool("IsPressing", pressed);
        }


        // --- Calculate current speed (common for all states) ---
        UpdateSpeedFromHeight();
        UpdateSpeedFromTime();

        float currentSpeed = _speed;
        if (startAccelerationDuration > 0f && _startElapsedTime < startAccelerationDuration)
        {
            _startElapsedTime += Time.fixedDeltaTime;
            float accelerationRatio = Mathf.Clamp01(_startElapsedTime / startAccelerationDuration);
            currentSpeed *= accelerationRatio;
        }

        // --- Determine velocity based on state ---
        if (requireFirstInputToStart && !_started)
        {
            if (flyStraightOnStart)
            {
                // 1. Pre-start, flying straight up.
                Vector2 velocity = (verticalSpeedMultiplier * currentSpeed * Vector2.up);
                rb.linearVelocity = velocity;
            }
            else
            {
                // 2. Pre-start, stationary.
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            // 3. Started, player has control.
            float horizSign = pressed ? -1f : 1f;
            Vector2 velocity = (horizSign * currentSpeed * direction) + (verticalSpeedMultiplier * currentSpeed * Vector2.up);
            rb.linearVelocity = velocity;
        }
    }

    private void UpdateSpeedFromHeight()
    {
        if (rocketTransform == null)
        {
            _speed = initialSpeed;
            return;
        }
        if (maxHeightSpeedIncrease <= 0f)
        {
            _speed = initialSpeed;
            return;
        }

        if (rocketTransform.position.y < maxHeightSpeedIncrease)
        {
            float delta = finalSpeed - initialSpeed;
            float denom = maxHeightSpeedIncrease / (Mathf.Abs(delta) < 0.0001f ? 1f : delta);
            _speed = initialSpeed + (rocketTransform.position.y / denom);
        }
        else
        {
            _speed = finalSpeed;
        }
    }

    private void UpdateSpeedFromTime()
    {
        if (!useTimeBasedSpeedIncrease) return;
        if (rocketTransform == null) return;
        if (timeBasedSpeedIncreaseIntervalSeconds <= 0f) return;

        if (rocketTransform.position.y >= timeBasedSpeedIncreaseMaxHeight)
            return;

        _elapsed += Time.deltaTime;
        if (_elapsed >= timeBasedSpeedIncreaseIntervalSeconds)
        {
            _speed += timeBasedSpeedIncreaseAmount;
            _elapsed = _elapsed % timeBasedSpeedIncreaseIntervalSeconds;
        }
    }

    private void MaybeConvertLegacySpeeds()
    {
        if (!autoScaleLegacySpeeds) return;
        if (_legacySpeedsAdjusted) return;

        bool looksLegacy = initialSpeed > legacySpeedThreshold || finalSpeed > legacySpeedThreshold;
        if (!looksLegacy) return;

        initialSpeed *= legacySpeedScale;
        finalSpeed *= legacySpeedScale;
        _legacySpeedsAdjusted = true;
    }

    private void ApplyGlobalSpeedConstants()
    {
        initialSpeed = GameConstants.RocketInitialSpeed;
        finalSpeed = GameConstants.RocketFinalSpeed;
        _legacySpeedsAdjusted = false;
    }

    /// <summary>
    /// Resets the controller to its pre-start state, allowing the start sequence to run again.
    /// </summary>
    public void ResetStart()
    {
        _started = false;
        _startElapsedTime = 0f;
        if (animator != null)
        {
            animator.SetBool("IsStarted", false);
        }
    }
}
