using System.Globalization;
using UnityEngine;

public class RocketMovement1 : MonoBehaviour
{

    public Rigidbody2D rb;
    private Vector2 direction = Vector2.left;
    float speed;
    public float initialSpeed = 125f;
    public float finalSpeed = 200f;
    public Transform rocketShip;
    public float maxHeightSpeedIncrease = 800f;
    // bool isClickedYet = false;
    public Animator animator;
    // private float elapsed = 0f;
    public Object destructableShip;
    public GameObject explosionEffect;
    public GameObject rocketShip1;
    // public float boundsX = 4.0f; //
    // private int score;


    /*
    void Update()
    {
        score = (1000f * (rocketShip.position.y + 3.5));
    }
    */

    void FixedUpdate()
    { 

        // To make the game played with spacebar
        if(Input.GetKey("space"))

        //// To make the game played with clicking
        //if (Input.GetMouseButton(0))
        {
            animator.SetBool("YetClicked", true);
        }

        // To make the game played with spacebar
        if(Input.GetKey("space"))

        //// To make the game played with clicking
        //if (Input.GetMouseButton(0))
        {
            rb.velocity = -speed * direction * Time.deltaTime + 2f * speed * Vector2.up * Time.deltaTime;
            animator.SetBool("ClickedCurrent", false);
        }
        else
        {
            rb.velocity = speed * direction * Time.deltaTime + 2f * speed * Vector2.up * Time.deltaTime;
            animator.SetBool("ClickedCurrent", true);
        }

        // elapsed += Time.deltaTime;



        if (rocketShip.position.y < maxHeightSpeedIncrease)
        {
            // Makes Initial Speed = 125 and Final Speed = 200
            speed = initialSpeed + (rocketShip.position.y / (maxHeightSpeedIncrease / (finalSpeed - initialSpeed))); 
        }

        //// Teleporting from one wall to the other
        // if (transform.position.x < -boundsX)
        // {
        //     transform.Translate(new Vector2(2 * boundsX, 0));
        // }
        // else if (transform.position.x > boundsX)
        // {
        //     transform.Translate(new Vector2(-2 * boundsX, 0));
        // }
    }

    

    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
        
        if (otherObj.tag == "Meteor")
        {
            // Destroy(this.gameObject);
            // CheckHighScore();
            ExplodeRocketShip();
            FindObjectOfType<GameManagerRS>().EndGame();
        }

        /*
        if (otherObj.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
        */
    }

    private void ExplodeRocketShip()
    {
        // replaces ship with broken ship when you hit a meteor
        GameObject destructable = (GameObject)Instantiate(destructableShip);
        Instantiate(explosionEffect, transform.position, transform.rotation);

        // maps the destructable onto the location of the old ship.
        destructable.transform.position = transform.position;

        if (rocketShip1.tag == "RocketShip")
        {
            // Destroy(this.gameObject);
            gameObject.SetActive(false);
        }
        
    }



    /*
    public void CheckHighScore()
    {

        if (score > double.Parse((PlayerPrefs.GetString("High Score", "1")), CultureInfo.InvariantCulture))
        {
            PlayerPrefs.SetString("High Score", score);

        }
    }

    */


}


