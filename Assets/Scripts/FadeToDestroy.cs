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

    protected static IEnumerator TimeEase(Action<float> action, float start, float target, float duration, Easer ease)
    {
        float elapsed = 0;
        var range = target - start;

        float startTime = Time.realtimeSinceStartup;
        while (elapsed < duration)
        {
            float deltaTime = Time.realtimeSinceStartup - startTime;
            elapsed = Mathf.MoveTowards(elapsed, duration, deltaTime);
            action(start + range * ease(elapsed / duration));
            yield return 0;
        }
        action(target);
    }
}
