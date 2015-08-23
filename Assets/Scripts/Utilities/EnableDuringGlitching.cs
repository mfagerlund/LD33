using UnityEngine;
using System.Collections;

public class EnableDuringGlitching : MonoBehaviour
{
    public GameObject[] objectsToEnable;

    public void Update()
    {
        foreach (GameObject o in objectsToEnable)
        {
            o.SetActive(Level.Instance.glitching);
        }
    }
}
