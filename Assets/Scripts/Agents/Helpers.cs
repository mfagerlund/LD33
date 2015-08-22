using UnityEngine;

public static class Helpers
{
    public static void DeleteAllChildren(Transform transform)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}