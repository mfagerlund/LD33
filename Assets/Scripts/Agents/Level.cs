using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }
    public GameObject agentHome;
    public LayerMask buildings;
    public bool monstersRevealed;
    public Transform garbageHome;

    [Header("Agent settings")]
    public float agentMaxSpeed = 1;
    public float agentMomentum = 0.95f;
    public float agentRotationSpeed = 360;
    public float agentHealthRegeneration = 0.5f;

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

    public void RestartCurrentLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}