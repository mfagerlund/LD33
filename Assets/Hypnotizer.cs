using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utilities;

public class Hypnotizer : MyMonoBehaviour
{
    public static Hypnotizer Instance { get; private set; }
    public float fadePeriod = 0.2f;

    private LineRenderer _lineRenderer;

    public void Start()
    {
        Instance = this;
        _lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    public void Update()
    {
        Instance = this;
    }

    public void ShowHypnotization(List<Agent> hypnotizedAgents)
    {
        List<Agent> allAgentsToConnect = new List<Agent>(hypnotizedAgents);
        allAgentsToConnect.AddRange(Level.Instance.GetSaviors());

        _lineRenderer.SetVertexCount(allAgentsToConnect.Count + 1);

        Agent agent = allAgentsToConnect.RandomItem();
        _lineRenderer.SetPosition(0, agent.Position);
        int p = 1;
        while (allAgentsToConnect.Any())
        {
            float bestDist = float.PositiveInfinity;
            Agent bestAgent = null;
            foreach (Agent agent1 in allAgentsToConnect)
            {
                float dist = Vector2.Distance(agent.Position, agent1.Position);
                if (dist < bestDist)
                {
                    bestAgent = agent1;
                    bestDist = dist;
                }
            }
            allAgentsToConnect.Remove(bestAgent);
            agent = bestAgent;
            _lineRenderer.SetPosition(p++, agent.Position);
        }

        StartCoroutine(
            TimeEase(f =>
            {
                Color color = new Color(227 / 255f, 255 / 255f, 150 / 255f, f);
                _lineRenderer.SetColors(color, color);
            }, 1, 0, fadePeriod, Ease.FromType(EaseType.CubeIn)));
    }
}
