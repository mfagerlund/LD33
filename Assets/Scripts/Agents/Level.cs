using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static Level Instance { get; private set; }
    public GameObject agentHome;
    public LayerMask buildings;
    public LayerMask saviors;
    public bool monstersRevealed;
    public Transform garbageHome;
    public RectTransform gameOverMessage;
    public Target SaviorAgentTypeTarget { get; set; }
    public Target HideFromSaviorsTarget { get; set; }
    public Target WeaponDropTarget { get; set; }
    public float LastViolence { get; private set; }

    [Header("Agent settings")]
    public float agentMaxSpeed = 1;
    public float agentMomentum = 0.95f;
    public float agentRotationSpeed = 360;
    public float agentHealthRegeneration = 0.5f;
    public float agentHypnotizationDistance = 50;
    public Material convertedMonsterMaterial;

    public void Start()
    {
        Instance = this;
        SaviorAgentTypeTarget = GetComponent<AgentTypeTarget>();
        HideFromSaviorsTarget = GetComponent<HideFromAgentTypeTarget>();
        WeaponDropTarget = GetComponent<WeaponDropTarget>();
        LastViolence = -1000;
    }

    public void Awake()
    {
        Instance = this;
    }

    public void Update()
    {
        if (GetAgents().All(a => a.agentType != AgentType.Savior))
        {
            gameOverMessage.gameObject.SetActive(true);
        }
    }

    public List<Agent> GetAgents()
    {
        // This should be cached!
        return agentHome.GetComponentsInChildren<Agent>().ToList();
    }

    public void RestartCurrentLevel()
    {
        Application.LoadLevel(Application.loadedLevel);
    }

    public void RebuildAis()
    {
        foreach (Agent agent in GetAgents())
        {
            agent.RebuildAi();
        }
    }

    public void RegisterViolence()
    {
        LastViolence = Time.time;
    }

    public List<Agent> GetSaviors()
    {
        return
            GetAgents()
                .Where(a => a.agentType == AgentType.Savior)
                .ToList();
    }
}