using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleBehavior : MonoBehaviour
{
    public Transform player;
    Rigidbody2D playerBody;
    public float pullRadius;
    public float intensity;
    public float distanceToPlayer;
    Vector2 pullForce;

    void Start()
    {
        if (player != null)
            playerBody = player.GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null || playerBody == null) return;

        distanceToPlayer = Vector2.Distance(player.position, transform.position);
        if (distanceToPlayer > pullRadius) return;
        if (distanceToPlayer <= 0.001f) return;

        pullForce = (transform.position - player.position).normalized / distanceToPlayer * intensity;
        playerBody.AddForce(pullForce, ForceMode2D.Force);
    }
}
