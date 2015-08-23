using System.Collections.Generic;
using Beehive.BehaviorTrees;

public class AgentBlackboard : BehaviourReflectionTreeBlackboard<AgentBlackboard>
{
    public AgentBlackboard(Agent agent)
        : base(null)
    {
        Agent = agent;
        Owner = this;
    }

    public Agent Agent { get; set; }

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

    public IEnumerator<TaskState> GoToTarget()
    {
        Agent.Target = Level.Instance.SaviorAgentTypeTarget;

        while (Agent.GoToTarget())
        {
            yield return TaskState.Running;
        }
        yield return TaskState.Failure;
    }
}