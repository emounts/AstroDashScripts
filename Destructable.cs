using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructable : MonoBehaviour
{
    // SerializeField makes it visible in the property inspector; similar to saying public
    [SerializeField]
    Vector2 forceDirection;

    [SerializeField]
    float torque;

    public Rigidbody2D rb2D;


    void Start()
    {
        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();
        if (rb2D == null)
            return;

        float randTorque = UnityEngine.Random.Range(-7, 7);
        rb2D.AddForce(forceDirection * 10);
        rb2D.AddTorque(randTorque + torque);
    }

    
}
