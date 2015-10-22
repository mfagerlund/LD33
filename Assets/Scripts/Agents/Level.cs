using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class Level : MyMonoBehaviour
{
    public static Level Instance { get; private set; }
    public GameObject agentHome;
    public LayerMask buildings;
    public LayerMask saviors;
    public bool monstersRevealed;
    public bool glitching;
    public Transform garbageHome;
    public RectTransform gameOverMessage;
    public Target SaviorAgentTypeTarget { get; set; }
    public Target HideFromSaviorsTarget { get; set; }
    public Target WeaponDropTarget { get; set; }
    public float LastViolence { get; private set; }

    public AudioClip[] glitchings;
    public AudioClip againstRegulations;

    [Header("Agent settings")]
    public float agentMaxSpeed = 1;
    public float agentMomentum = 0.95f;
    public float agentRotationSpeed = 360;
    public float agentHealthRegeneration = 0.5f;
    public float agentHypnotizationDistance = 50;
    public Material convertedMonsterMaterial;
    public int AgentsKilled { get; set; }

    private bool _selected = false;

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
        if (!_selected)
        {
            _selected = true;
            foreach (Agent agent in GetSaviors().Take(3))
            {
                SelectionManager.Instance.SelectAgent(agent);
            }
        }
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

    public void TurnOffGoggles()
    {
        AudioSource.PlayClipAtPoint(againstRegulations, Vector2.zero, 0.6f);
    }


    public void Quit()
    {
        Debug.Log("Quitting");

#if UNITY_STANDALONE_WIN
        if (!Application.isEditor)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
#else
        Application.Quit();
#endif
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

    public void RegisterDeath(Agent agent)
    {
        AgentsKilled++;

        if (AgentsKilled == 10)
        {
            StartGlitch(0.25f, 30f);
        }
        if (AgentsKilled == 25)
        {
            StartGlitch(0.6f, 30f);
        }
        if (AgentsKilled == 50)
        {
            StartGlitch(0.9f, 45f);
        }
        if (AgentsKilled == 70)
        {
            glitching = true;
            monstersRevealed = true;
        }
    }

    private void StartGlitch(float glitchPercentage, float glitchTime)
    {
        glitching = true;
        AudioSource.PlayClipAtPoint(glitchings.RandomItem(), Vector2.zero, 0.6f);
        StartCoroutine(
            TimeEase(
                f =>
                {
                    monstersRevealed = Random.Range(0f, 1f) <= f;
                }, glitchPercentage, 0, glitchTime, EaseType.CubeOut));

        Invoke(() =>
        {
            monstersRevealed = false;
            glitching = false;
        }, glitchTime + 0.1f);
    }
}