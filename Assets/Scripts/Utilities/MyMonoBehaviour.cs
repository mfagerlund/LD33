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