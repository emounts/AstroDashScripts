using UnityEngine;

public class CollisionScript1 : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject otherObj = collision.gameObject;
    }
}
