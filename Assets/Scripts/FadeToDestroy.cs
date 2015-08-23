using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeToDestroy : MyMonoBehaviour
{
    public float timeToStartFading = 10;
    public float fadeTime = 2;
    private SpriteRenderer _spriteRenderer;
    private CanvasGroup _canvasGroup;

    public void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _canvasGroup = GetComponent<CanvasGroup>();
        Invoke(FadeOut, timeToStartFading);
        Destroy(gameObject, timeToStartFading + fadeTime);
    }

    private void FadeOut()
    {
        if (_spriteRenderer != null)
        {
            FadeOutSpriteRenderer();
        }

        if (_canvasGroup != null)
        {
            FadeOutCanvasGroup();
        }
    }

    private void FadeOutCanvasGroup()
    {
        StartCoroutine(TimeEase(f => { _canvasGroup.alpha = f; }, _canvasGroup.alpha, 0, fadeTime, Ease.FromType(EaseType.CubeOut)));
    }

    private void FadeOutSpriteRenderer()
    {
        Color color = _spriteRenderer.color;
        StartCoroutine(
            TimeEase(f =>
            {
                color.a = f;
                _spriteRenderer.color = color;
            }, color.a, 0, fadeTime, Ease.FromType(EaseType.CubeOut)));
    }
}
