using UnityEngine;

public class StarrySkyFollow : MonoBehaviour
{
    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    public Vector3 offset;

    void Update()
    {
        if (skinManager == null) skinManager = SkinManager.Instance;
        
        if (skinManager != null && skinManager.CurrentRocketTransform != null)
        {
            transform.position = skinManager.CurrentRocketTransform.position + offset;
        }
    }
}