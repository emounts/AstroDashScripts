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
    private GameObject _currentDestructible;
    private GameObject _currentExplosion;

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
        _controller.flyStraightOnStart = true;
        _controller.requireFirstInputToStart = true; 

        if (skinManager != null && skinManager.CurrentSkin != null)
            _controller.ApplySkinTuning(skinManager.CurrentSkin);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
        if (otherObj.tag == "Meteor")
        {
            GameEvents.RaisePlayerCrashed();
            ExplodeRocketShip();
        }
    }

    private void ExplodeRocketShip()
    {
        if (destructableShip != null)
        {
            _currentDestructible = (GameObject)Instantiate(destructableShip);
            _currentDestructible.transform.position = transform.position;
        }

        if (explosionEffect != null)
        {
            _currentExplosion = Instantiate(explosionEffect, transform.position, transform.rotation);
        }

        if (rocketShip1 != null && rocketShip1.tag == "RocketShip")
            gameObject.SetActive(false);
    }

    public void Respawn()
    {
        // Cleanup debris
        if (_currentDestructible != null)
        {
            Destroy(_currentDestructible);
            _currentDestructible = null;
        }
        if (_currentExplosion != null)
        {
            Destroy(_currentExplosion);
            _currentExplosion = null;
        }

        // Reactivate the rocket ship visuals.
        if (rocketShip1 != null)
        {
            rocketShip1.SetActive(true);
        }
        gameObject.SetActive(true);
        transform.rotation = Quaternion.identity; // Reset orientation
        if (rb != null)
        {
            rb.SetRotation(0f); // Explicitly reset physics rotation
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Reset movement state.
        direction = Vector2.left;

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
