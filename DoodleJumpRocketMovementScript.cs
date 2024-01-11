using UnityEngine;

public class DoodleJumpRocketMovementScript : MonoBehaviour
{

    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    public float speed = 50f;
    public float boundsX = 2.9f;


    bool isClickedYet = false;




    void FixedUpdate()
    {

        if (Input.GetMouseButton(0))
        {
            isClickedYet = true;
            Debug.Log("CLICKED");
        }

        if (isClickedYet)
        {




            if (Input.GetMouseButton(0))
            {
                rb.velocity = -speed * direction * Time.deltaTime;
            }
            else
            {
                rb.velocity = speed * direction * Time.deltaTime;
                Debug.Log("Clicked");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
        Debug.Log("Collided with: " + otherObj.tag);

        if (otherObj.tag == "Meteor")
        {
            Destroy(this.gameObject);
        }
        if (otherObj.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
    }




    

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
    
}


