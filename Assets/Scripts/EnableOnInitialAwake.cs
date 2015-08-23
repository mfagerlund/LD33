using UnityEngine;

public class EnableOnInitialAwake : MonoBehaviour
{
    public GameObject objectToEnable;
    public MonoBehaviour componentToEnable;
    private bool _hasBeenAwoken;

    public void Awake()
    {
        if (!_hasBeenAwoken)
        {
            _hasBeenAwoken = true;

            if (componentToEnable != null)
            {
                componentToEnable.enabled = true;
            }

            if (objectToEnable != null)
            {
                objectToEnable.SetActive(true);
            }

            if (componentToEnable == null && objectToEnable == null)
            {
                Debug.Log("Found not object or component to enable!");
            }
        }
    }
}