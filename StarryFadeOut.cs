using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarryFadeOut : MonoBehaviour
{
    public float fadeOutTime = 1.0f;
    public Transform rocketShip;
    public float fadeHeight = 1500f;

        public void Update()
        {
            if (rocketShip.position.y > fadeHeight)
            {
                StartCoroutine(SpriteFadeOut(GetComponent<SpriteRenderer>()));
            }
        }

        public IEnumerator SpriteFadeOut(SpriteRenderer _sprite)
        {
            Color tmpColor = _sprite.color;

            while (tmpColor.a > 0f)
            {
                tmpColor.a -= Time.deltaTime / fadeOutTime;
                _sprite.color = tmpColor;

                if (tmpColor.a <= 0f)
                {
                    tmpColor.a = 0.0f;
                }

                yield return null;
            }
            _sprite.color = tmpColor;
        }
}


