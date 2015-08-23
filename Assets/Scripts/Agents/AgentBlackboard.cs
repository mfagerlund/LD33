using System.Collections.Generic;
using Beehive.BehaviorTrees;
using UnityEngine;

public class AgentBlackboard : BehaviourReflectionTreeBlackboard<AgentBlackboard>
{
    public AgentBlackboard(Agent agent)
        : base(null)
    {
        Agent = agent;
        Owner = this;
    }

    public Agent Agent { get; set; }

    public bool Hypnotized { get { return Agent.Hypnotized; } }

    public float TimeSinceViolence
    {
        get
        {
            return Time.time - Level.Instance.LastViolence;
        }
    }

    public float DistanceToEnemy
    {
        get
        {
            return
                Level
                    .Instance
                    .SaviorAgentTypeTarget
                    .GetWalkingDistanceFrom(Agent.Position);
        }
    }

    public IEnumerator<TaskState> SelectEnemyTarget()
    {
        Agent.Target = Level.Instance.SaviorAgentTypeTarget;
        yield return TaskState.Success;
    }

    public IEnumerator<TaskState> SelectHideFromSavioursTarget()
    {
        Agent.Target = Level.Instance.HideFromSaviorsTarget;
        yield return TaskState.Success;
    }

    public IEnumerator<TaskState> GoToTarget()
    {
        Agent.Target = Level.Instance.SaviorAgentTypeTarget;

        while (Agent.GoToTarget())
        {
            yield return TaskState.Running;
        }
        yield return TaskState.Failure;
    }

    public IEnumerator<TaskState> Wander()
    {
        Agent.Target = null;
        Agent.RotateTowards(Mathf.PerlinNoise(Agent.Position.x, Agent.Position.y + Time.time / 30) * 360);
        Agent.WantedSpeed = Vector2.zero;
        yield return TaskState.Success;
    }
}