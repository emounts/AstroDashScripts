using UnityEngine;

public class FollowRocketShip : MonoBehaviour
{
    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    public Vector3 offset;

    void Update()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
        
        if (skinManager != null)
        {
            Transform t = skinManager.CurrentRocketTransform;
            if (t != null)
            {
                transform.position = t.position + offset;
            }
        }
    }
}