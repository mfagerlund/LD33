using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }
    public GameObject agentHome;

    [Header("Agent settings")]
    public float agentMaxSpeed = 1;
    public float agentMomentum = 0.95f;
    
    public void Start()
    {
        Instance = this;
    }

    public void Awake()
    {
        Instance = this;
    }

    public List<Agent> GetAgents()
    {
        return agentHome.GetComponentsInChildren<Agent>().ToList();
    }

}