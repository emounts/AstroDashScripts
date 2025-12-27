using UnityEngine;

public class DoodleJumpRocketMovementScript : MonoBehaviour
{
    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    public float speed = GameConstants.RocketInitialSpeed;
    public float boundsX = 2.9f;

    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;
    private RocketController _controller;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (skinManager == null) skinManager = SkinManager.Instance;

        _controller = GetComponent<RocketController>();
        if (_controller == null) _controller = gameObject.AddComponent<RocketController>();

        _controller.rb = rb;
        _controller.rocketTransform = transform;
        _controller.direction = direction;
        _controller.useMouseInput = true;
        _controller.requireFirstInputToStart = true;

        // DoodleJump mode: horizontal only.
        _controller.verticalSpeedMultiplier = 0f;
        _controller.initialSpeed = GameConstants.RocketInitialSpeed;
        _controller.finalSpeed = GameConstants.RocketFinalSpeed;
        _controller.maxHeightSpeedIncrease = 0f;

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
        if (otherObj.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        Vector3 temp = transform.position;
        if (temp.x < -boundsX)
        {
            transform.Translate(new Vector3(2 * boundsX, 0, 0));
        }
        else if (temp.x > boundsX)
        {
            transform.Translate(new Vector3(-2 * boundsX, 0, 0));
        }
    }
}
