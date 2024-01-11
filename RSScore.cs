using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// TMPro is needed for the text I'm using
using TMPro;
using System.Globalization;
using System;

public class RSScore : MonoBehaviour
{
    public Transform rocketShipPosition;
    public Transform Ship2Position;
    public Transform Ship3Position;
    public Transform MeteorPosition;
    public Transform LockedShipPosition;
    public GameObject rocketShip;
    public GameObject Ship2;
    public GameObject Ship3;
    public GameObject Meteor;
    public GameObject LockedShip;
    public TextMeshProUGUI scoreText;

    private double score;
    private string scoreString;



    void Update()
    {

    if(rocketShip.activeSelf)
        {
            score = Math.Round((1000f * (rocketShipPosition.position.y + 3.5)));
            scoreString = score.ToString();

            scoreText.text = (1000f * (rocketShipPosition.position.y + 3.5)).ToString("0");
            // Debug.Log(PlayerPrefs.GetString("High Score", "1"));

            CheckHighScore();
        }
        else if(Ship2.activeSelf)
        {
            score = Math.Round((1000f * (Ship2Position.position.y + 3.5)));
            scoreString = score.ToString();

            scoreText.text = (1000f * (Ship2Position.position.y + 3.5)).ToString("0");
            // Debug.Log(PlayerPrefs.GetString("High Score", "1"));

            CheckHighScore();      
        }

        else if(Ship3.activeSelf)
        {
            score = Math.Round((1000f * (Ship3Position.position.y + 3.5)));
            scoreString = score.ToString();

            scoreText.text = (1000f * (Ship3Position.position.y + 3.5)).ToString("0");
            // Debug.Log(PlayerPrefs.GetString("High Score", "1"));

            CheckHighScore();
        }
        else if(Meteor.activeSelf)
        {
            score = Math.Round((1000f * (MeteorPosition.position.y + 3.5)));
            scoreString = score.ToString();

            scoreText.text = (1000f * (MeteorPosition.position.y + 3.5)).ToString("0");
            // Debug.Log(PlayerPrefs.GetString("High Score", "1"));

            CheckHighScore();
        }
        else if(LockedShip.activeSelf)
        {
            score = Math.Round((1000f * (LockedShipPosition.position.y + 3.5)));
            scoreString = score.ToString();

            scoreText.text = (1000f * (LockedShipPosition.position.y + 3.5)).ToString("0");
            // Debug.Log(PlayerPrefs.GetString("High Score", "1"));

            CheckHighScore();
        }
    }

    private void CheckHighScore()
    {

        if (score > double.Parse((PlayerPrefs.GetString("High Score", "1")), CultureInfo.InvariantCulture))
        {
            PlayerPrefs.SetString("High Score",scoreString);
            
        }
    }
}
