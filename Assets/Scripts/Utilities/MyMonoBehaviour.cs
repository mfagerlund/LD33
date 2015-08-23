using System;
using System.Collections;
using UnityEngine;

public abstract class MyMonoBehaviour : MonoBehaviour
{
    public void ClearAllChildren(Transform transformToClear = null, Func<Transform, bool> predicate = null)
    {
        if (transformToClear == null)
        {
            transformToClear = transform;
        }
        for (int i = 0; i < transformToClear.childCount; i++)
        {
            if (predicate != null && !predicate(transform))
            {
                continue;
            }
            Destroy(transformToClear.GetChild(i).gameObject);
        }
    }

    public IEnumerator StartDelayedCoroutine(
        Func<IEnumerator> coRoutine,
        float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        StartCoroutine(coRoutine());
    }

    public Coroutine StartCoroutine(
        Func<IEnumerator> coRoutine)
    {
        return StartCoroutine(coRoutine());
    }

    public void Invoke(Action action, float delaySeconds)
    {
        StartCoroutine(DelayedActionCoroutine(action, delaySeconds));
    }

    public void Invoke(
        Action action,
        Func<bool> predicate)
    {
        StartCoroutine(PredicatedActionCoroutine(action, predicate));
    }


    public TType InstantiateAtMe<TType>(TType prefab) where TType : Component
    {
        TType result = (TType)Instantiate(prefab, transform.position, Quaternion.identity);
        result.transform.SetParent(transform, false);
        return result;
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

    private static IEnumerator DelayedActionCoroutine(
        Action action,
        float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        action();
    }

    private static IEnumerator PredicatedActionCoroutine(
        Action action,
        Func<bool> predicate)
    {
        while (!predicate())
        {
            yield return null;
        }
        action();
    }
}