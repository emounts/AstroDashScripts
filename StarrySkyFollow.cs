using UnityEngine;

public class StarrySkyFollow : MonoBehaviour
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
    public Vector3 offset;

    void Update()
    {
        if(rocketShip.activeSelf)
        {
            transform.position = rocketShipPosition.position + offset;
        }
        else if(Ship2.activeSelf)
        {
            transform.position = Ship2Position.position + offset;
        }
        else if(Ship3.activeSelf)
        {
            transform.position = Ship3Position.position + offset;
        }
        else if(Meteor.activeSelf)
        {
            transform.position = MeteorPosition.position + offset;
        }
        else if(LockedShip.activeSelf)
        {
            transform.position = LockedShipPosition.position + offset;
        }
    }
}

    
