using System;
using System.Collections;
using UnityEngine;

public class FadeToDestroy : MyMonoBehaviour
{
    public float timeToStartFading = 10;
    public float fadeTime = 2;
    private SpriteRenderer _spriteRenderer;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        Invoke(FadeOut, timeToStartFading);
        Destroy(gameObject, timeToStartFading + fadeTime);
    }

    private void FadeOut()
    {
        Color color = _spriteRenderer.color;
        float startAlpha = color.a;
        StartCoroutine(
            TimeEase(f =>
            {
                color.a = f;
                _spriteRenderer.color = color;
            }, startAlpha, 0, fadeTime, Ease.FromType(EaseType.CubeOut)));
    }  
}
