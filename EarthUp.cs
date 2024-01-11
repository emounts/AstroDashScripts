using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthUp : MonoBehaviour
{
    public Rigidbody2D rb;
    public float upSpeed = 80f;
    public float destroyDelay = 1.5f;
    // private bool isClickedYet = false;



    void FixedUpdate()
    {
        rb.velocity = upSpeed * Vector2.up * Time.deltaTime;

        if (gameObject.tag ==  "Earth")
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}
