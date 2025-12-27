using System.Collections;
using UnityEngine;

public class StarryFadeOut : MonoBehaviour
{
    public float fadeOutTime = 1.0f;
    public Transform rocketShip;
    public float fadeHeight = 1500f;

    [Header("Optional: data-driven")]
    [SerializeField] private SkinManager skinManager;

    private bool _isFading;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Update()
    {
        if (_isFading) return;
        if (_sr == null) return;

        if (skinManager == null) skinManager = SkinManager.Instance;
        Transform current = skinManager != null ? skinManager.CurrentRocketTransform : null;
        Transform target = current != null ? current : rocketShip;
        if (target == null) return;

        if (target.position.y > fadeHeight)
        {
            _isFading = true;
            StartCoroutine(SpriteFadeOut(_sr));
        }
    }

    public IEnumerator SpriteFadeOut(SpriteRenderer sprite)
    {
        Color tmpColor = sprite.color;
        while (tmpColor.a > 0f)
        {
            tmpColor.a -= Time.deltaTime / fadeOutTime;
            sprite.color = tmpColor;

            if (tmpColor.a <= 0f)
                tmpColor.a = 0.0f;

            yield return null;
        }
        sprite.color = tmpColor;
    }
}
