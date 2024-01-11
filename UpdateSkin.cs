using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class UpdateSkin : MonoBehaviour
{

    public GameObject ActivateSkin1;
    public GameObject ActivateSkin2;
    public GameObject ActivateSkin3;
    public GameObject ActivateSkin4;
    public GameObject ActivateSkin5;


    void Start()
    {
        if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 0)
        {
                ActivateSkin1.SetActive(true);
                ActivateSkin2.SetActive(false);
                ActivateSkin3.SetActive(false);
                ActivateSkin4.SetActive(false);
                ActivateSkin5.SetActive(false);
           
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 1)
        {
                ActivateSkin2.SetActive(true);
                ActivateSkin1.SetActive(false);
                ActivateSkin3.SetActive(false);
                ActivateSkin4.SetActive(false);
                ActivateSkin5.SetActive(false);
            
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 2)
        {
                ActivateSkin3.SetActive(true);
                ActivateSkin2.SetActive(false);
                ActivateSkin1.SetActive(false);
                ActivateSkin4.SetActive(false);
                ActivateSkin5.SetActive(false);
            
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 3)
        {
                ActivateSkin4.SetActive(true);
                ActivateSkin2.SetActive(false);
                ActivateSkin3.SetActive(false);
                ActivateSkin1.SetActive(false);
                ActivateSkin5.SetActive(false);
            
        }
        else if (PlayerPrefs.GetInt("Skin Menu Position", 1) == 4)
        {
                ActivateSkin5.SetActive(true);
                ActivateSkin2.SetActive(false);
                ActivateSkin3.SetActive(false);
                ActivateSkin4.SetActive(false);
                ActivateSkin1.SetActive(false);
            
        }



    }
}