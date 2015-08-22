using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }

    public void Start()
    {
        Instance = this;
    }

    public void Awake()
    {
        Instance = this;
    }

    public float maxAgentSpeed=1;
}