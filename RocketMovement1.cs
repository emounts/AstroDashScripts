using UnityEngine;

public class RocketMovement1 : MonoBehaviour
{
    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    public float initialSpeed = GameConstants.RocketInitialSpeed;
    public float finalSpeed = GameConstants.RocketFinalSpeed;
    public Transform rocketShip;
    public float maxHeightSpeedIncrease = 800f;
    public Object destructableShip;
    public GameObject explosionEffect;
    public GameObject rocketShip1;

    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;
    private RocketController _controller;

    private void Awake()
    {
        initialSpeed = GameConstants.RocketInitialSpeed;
        finalSpeed = GameConstants.RocketFinalSpeed;

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rocketShip == null) rocketShip = transform;
        if (skinManager == null) skinManager = SkinManager.Instance;

        _controller = GetComponent<RocketController>();
        if (_controller == null) _controller = gameObject.AddComponent<RocketController>();

        _controller.rb = rb;
        _controller.rocketTransform = rocketShip;
        _controller.direction = direction;
        _controller.useMouseInput = false;
        _controller.keyboardKey = KeyCode.Space;
        _controller.initialSpeed = GameConstants.RocketInitialSpeed;
        _controller.finalSpeed = GameConstants.RocketFinalSpeed;
        _controller.maxHeightSpeedIncrease = maxHeightSpeedIncrease;
        _controller.verticalSpeedMultiplier = 2f;

        if (skinManager != null && skinManager.CurrentSkin != null)
            _controller.ApplySkinTuning(skinManager.CurrentSkin);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
        if (otherObj.tag == "Meteor")
        {
            ExplodeRocketShip();
            GameEvents.RaisePlayerCrashed();
        }
    }

    private void ExplodeRocketShip()
    {
        if (destructableShip != null)
        {
            GameObject destructable = (GameObject)Instantiate(destructableShip);
            destructable.transform.position = transform.position;
        }

        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, transform.rotation);

        if (rocketShip1 != null && rocketShip1.tag == "RocketShip")
            gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Reactivate the rocket ship visuals.
        if (rocketShip1 != null)
        {
            rocketShip1.SetActive(true);
        }
        gameObject.SetActive(true);

        // Reset movement state.
        direction = Vector2.left;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset the controller's direction.
        if (_controller != null)
        {
            _controller.direction = Vector2.left;
            _controller.ResetStart(); 
        }

        // Optional: Reset animation state if an Animator is used.
        Animator anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play("RS_Up"); // Go back to the initial state.
        }
    }
}
