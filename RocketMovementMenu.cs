using UnityEngine;

public class RocketMovementMenu : MonoBehaviour
{

    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    public float speed = 50f;
    public float incrementSpeed = .73f;
    public Transform rocketShip;
    public float maxHeightSpeedIncrease = 670f;
    // bool isClickedYet = false;
    public Animator animator;
    private float elapsed = 0f;

    // public float boundsX = 2.9f;


    void FixedUpdate()
    {



        if (Input.GetMouseButton(0))
        {

            animator.SetBool("YetClicked", true);
        }


        // if (isClickedYet)
        // {
        if (Input.GetMouseButton(0))
        {
            rb.velocity = -speed * direction * Time.deltaTime + 2f * speed * Vector2.up * Time.deltaTime;
            animator.SetBool("ClickedCurrent", false);
        }
        else
        {
            rb.velocity = speed * direction * Time.deltaTime + 2f * speed * Vector2.up * Time.deltaTime;
            animator.SetBool("ClickedCurrent", true);
        }

        elapsed += Time.deltaTime;

        // }


        if (rocketShip.position.y < maxHeightSpeedIncrease)
        {
            if (elapsed >= 1f)
            {
                speed += incrementSpeed;
                elapsed = elapsed % 1f;
            }
        }
    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;

        if (otherObj.tag == "Meteor")
        {
            Destroy(this.gameObject);
            FindObjectOfType<GameManagerRS>().EndGame();
        }


        /*
        if (otherObj.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
        */
    }



    /*


    private void Update()
    {
        // Teleporting from one wall to the other
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
    */

}


