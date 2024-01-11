using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonUp : MonoBehaviour
{
    public Transform moonT;
    public GameObject moonGO;
    public Rigidbody2D moonRB;
    public Transform rs1;
    public Transform rs2;
    public Transform rs3;
    public Transform rs4;
    public Transform rs5;
    public GameObject RS1;
    public GameObject RS2;
    public GameObject RS3;
    public GameObject RS4;
    public GameObject RS5;

    private float rocketSpeed;
    public float slowingSpeed = 0.5f;

    
    void Update()
    {
        if(RS1.activeSelf)
        {
            StartCoroutine(CalculateSpeedRS1());
            moonRB.velocity = (rocketSpeed - slowingSpeed) * Vector2.up; //* Time.deltaTime;
        }

        else if (RS2.activeSelf)
        {
            StartCoroutine(CalculateSpeedRS2());
            moonRB.velocity = (rocketSpeed - slowingSpeed) * Vector2.up; //* Time.deltaTime;
        }

        else if (RS3.activeSelf)
        {
            StartCoroutine(CalculateSpeedRS3());
            moonRB.velocity = (rocketSpeed - slowingSpeed) * Vector2.up; //* Time.deltaTime;
        }

        else if (RS4.activeSelf)
        {
            StartCoroutine(CalculateSpeedRS4());
            moonRB.velocity = (rocketSpeed - slowingSpeed) * Vector2.up; //* Time.deltaTime;
        }

        else if (RS5.activeSelf)
        {
            StartCoroutine(CalculateSpeedRS5());
            moonRB.velocity = (rocketSpeed - slowingSpeed) * Vector2.up; //* Time.deltaTime;
        }
        else 
        {
            moonRB.velocity = Vector2.zero * Time.deltaTime;
            Debug.Log("Collision!");
        }

        // if(rs1.position.y - moonT.position.y > 30f) 
        // {
        //     moonGO.SetActive(false);
        // }
                
            
    }

    IEnumerator CalculateSpeedRS1()
    {
        Vector3 lastPosition = rs1.position;
        yield return new WaitForFixedUpdate();
        rocketSpeed = (lastPosition - rs1.position).magnitude / Time.deltaTime;
    }

    IEnumerator CalculateSpeedRS2()
    {
        Vector3 lastPosition = rs2.position;
        yield return new WaitForFixedUpdate();
        rocketSpeed = (lastPosition - rs2.position).magnitude / Time.deltaTime;
    }

    IEnumerator CalculateSpeedRS3()
    {
        Vector3 lastPosition = rs3.position;
        yield return new WaitForFixedUpdate();
        rocketSpeed = (lastPosition - rs3.position).magnitude / Time.deltaTime;
    }

    IEnumerator CalculateSpeedRS4()
    {
        Vector3 lastPosition = rs4.position;
        yield return new WaitForFixedUpdate();
        rocketSpeed = (lastPosition - rs4.position).magnitude / Time.deltaTime;
    }

    IEnumerator CalculateSpeedRS5()
    {
        Vector3 lastPosition = rs5.position;
        yield return new WaitForFixedUpdate();
        rocketSpeed = (lastPosition - rs5.position).magnitude / Time.deltaTime;
    }
}
