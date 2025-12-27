using UnityEngine;

public class RocketMovementMenu : MonoBehaviour
{
    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    public float speed = GameConstants.RocketInitialSpeed;
    public float incrementSpeed = .73f;
    public Transform rocketShip;
    public float maxHeightSpeedIncrease = 670f;

    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;
    private RocketController _controller;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rocketShip == null) rocketShip = transform;
        if (skinManager == null) skinManager = SkinManager.Instance;

        _controller = GetComponent<RocketController>();
        if (_controller == null) _controller = gameObject.AddComponent<RocketController>();

        _controller.rb = rb;
        // Pregame UI: rocket should remain stationary (only animate).
        _controller.enableMovement = false;
        if (rb != null) rb.linearVelocity = UnityEngine.Vector2.zero;
        _controller.rocketTransform = rocketShip;
        _controller.direction = direction;
        _controller.useMouseInput = true;
        _controller.keyboardKey = KeyCode.Space;
        _controller.verticalSpeedMultiplier = 2f;
        // Menu idle: do not start moving until the player taps/clicks.
        _controller.requireFirstInputToStart = true;

        // Menu behavior: time-based speed increase while below a max height.
        _controller.initialSpeed = GameConstants.RocketInitialSpeed;
        _controller.finalSpeed = GameConstants.RocketFinalSpeed;
        _controller.maxHeightSpeedIncrease = maxHeightSpeedIncrease;
        _controller.useTimeBasedSpeedIncrease = true;
        _controller.timeBasedSpeedIncreaseMaxHeight = maxHeightSpeedIncrease;
        _controller.timeBasedSpeedIncreaseIntervalSeconds = 1f;
        _controller.timeBasedSpeedIncreaseAmount = incrementSpeed;

        if (skinManager != null && skinManager.CurrentSkin != null)
            _controller.ApplySkinTuning(skinManager.CurrentSkin);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
        if (otherObj.tag == "Meteor")
        {
            Destroy(this.gameObject);
            GameEvents.RaisePlayerCrashed();
        }
    }
}
