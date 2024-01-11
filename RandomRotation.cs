using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour
{

    private Transform meteorTransform;
    private float randRotation;

    void Start()
    {
        meteorTransform = GetComponent<Transform>();
        randRotation = UnityEngine.Random.Range(-180, 180);
        meteorTransform.Rotate(0, 0, randRotation);
    }

    
}
