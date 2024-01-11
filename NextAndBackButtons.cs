using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class NextAndBackButtons : MonoBehaviour
{
    public GameObject Ship01;
    public GameObject Ship02;
    public GameObject Ship03;
    public GameObject Ship04;
    public GameObject Ship05;
    public GameObject MenuToActivate;
    public GameObject ActivateSkin1;
    public GameObject ActivateSkin2;
    public GameObject ActivateSkin3;
    public GameObject ActivateSkin4;
    public GameObject ActivateSkin5;
    public GameObject MenuToDeactivate;
    public Transform Ship1;
    public Transform Ship2;
    public Transform Ship3;
    public Transform Ship4;
    public Transform Ship5;
    private Vector3 downLeft = new Vector3(-1.9f, -.5f, 0f);
    private Vector3 upMiddle = new Vector3(0f, -0f, 0f);
    private Vector3 downRight = new Vector3(1.9f, -.5f, 0f);
    private Vector3 outOfView = new Vector3(20f, 0f, 0f);
    private int numberOfRightMovements = 0;


    void Start()
    {
        if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 0)
        {
            Ship01.transform.position = upMiddle;
            Ship02.transform.position = downRight;
            Ship03.transform.position = outOfView;
            Ship04.transform.position = outOfView;
            Ship05.transform.position = outOfView;
            numberOfRightMovements = 0;
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 1)
        {
            Ship01.transform.position = downLeft;
            Ship02.transform.position = upMiddle;
            Ship03.transform.position = downRight;
            Ship04.transform.position = outOfView;
            Ship05.transform.position = outOfView;
            numberOfRightMovements = 1;
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 2)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = downLeft;
            Ship03.transform.position = upMiddle;
            Ship04.transform.position = downRight;
            Ship05.transform.position = outOfView;
            numberOfRightMovements = 2;
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 3)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = outOfView;
            Ship03.transform.position = downLeft;
            Ship04.transform.position = upMiddle;
            Ship05.transform.position = downRight;
            numberOfRightMovements = 3;
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 4)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = outOfView;
            Ship03.transform.position = outOfView;
            Ship04.transform.position = downLeft;
            Ship05.transform.position = upMiddle;
            numberOfRightMovements = 4;
        }
        
        // Ship01.transform.position = upMiddle;
        // Ship02.transform.position = downRight;
        // Ship03.transform.position = outOfView;
        // Ship04.transform.position = outOfView;
        // Ship05.transform.position = outOfView;

        PlayerPrefs.SetInt("Skin Menu Position", 0);
    }

    public void NextOption()
    {
        // Ship01.tag == "Ship1"
        if (numberOfRightMovements == 0)
        {
            Ship01.transform.position = downLeft;
            Ship02.transform.position = upMiddle;
            Ship03.transform.position = downRight;
            Ship04.transform.position = outOfView;
            Ship05.transform.position = outOfView;
        }
        else if (numberOfRightMovements == 1)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = downLeft;
            Ship03.transform.position = upMiddle;
            Ship04.transform.position = downRight;
            Ship05.transform.position = outOfView;
        }
        else if (numberOfRightMovements == 2)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = outOfView;
            Ship03.transform.position = downLeft;
            Ship04.transform.position = upMiddle;
            Ship05.transform.position = downRight;
        }
        else if (numberOfRightMovements == 3)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = outOfView;
            Ship03.transform.position = outOfView;
            Ship04.transform.position = downLeft;
            Ship05.transform.position = upMiddle;
        }
        else if (numberOfRightMovements == 4)
        {
            numberOfRightMovements--;
        }

        numberOfRightMovements++;
        PlayerPrefs.SetInt("Skin Menu Position", numberOfRightMovements);
    }


    public void BackOption()
    {
        if (numberOfRightMovements == 0)
        {
            numberOfRightMovements++;
        }
        else if (numberOfRightMovements == 1)
        {
            Ship01.transform.position = upMiddle;
            Ship02.transform.position = downRight;
            Ship03.transform.position = outOfView;
            Ship04.transform.position = outOfView;
            Ship05.transform.position = outOfView;
        }
        else if (numberOfRightMovements == 2)
        {
            Ship01.transform.position = downLeft;
            Ship02.transform.position = upMiddle;
            Ship03.transform.position = downRight;
            Ship04.transform.position = outOfView;
            Ship05.transform.position = outOfView;
        }
        else if (numberOfRightMovements == 3)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = downLeft;
            Ship03.transform.position = upMiddle;
            Ship04.transform.position = downRight;
            Ship05.transform.position = outOfView;
        }
        else if (numberOfRightMovements == 4)
        {
            Ship01.transform.position = outOfView;
            Ship02.transform.position = outOfView;
            Ship03.transform.position = downLeft;
            Ship04.transform.position = upMiddle;
            Ship05.transform.position = downRight;
        }

        numberOfRightMovements--;
        PlayerPrefs.SetInt("Skin Menu Position", numberOfRightMovements);

    }


    public void SelectedShip()
    {
        if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 0)
        {
            if (Ship01.tag == "Locked")
            {
            }
            else
            {
                MenuToActivate.SetActive(true);
                ActivateSkin1.SetActive(true);
                MenuToDeactivate.SetActive(false);
            }
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 1)
        {
            if (Ship02.tag == "Locked")
            {
            }
            else
            {
                MenuToActivate.SetActive(true);
                ActivateSkin2.SetActive(true);
                MenuToDeactivate.SetActive(false);
            }
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 2)
        {
            if (Ship03.tag == "Locked")
            {
            }
            else
            {
                MenuToActivate.SetActive(true);
                ActivateSkin3.SetActive(true);
                MenuToDeactivate.SetActive(false);
            }
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 3)
        {
            if (Ship04.tag == "Locked")
            {
            }
            else
            {
                MenuToActivate.SetActive(true);
                ActivateSkin4.SetActive(true);
                MenuToDeactivate.SetActive(false);
            }
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 4)
        {
            if (Ship05.tag == "Locked")
            {
            }
            else
            {
                MenuToActivate.SetActive(true);
                ActivateSkin5.SetActive(true);
                MenuToDeactivate.SetActive(false);
            }
        }
           


    }
}