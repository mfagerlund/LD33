using System.Collections.Generic;
using Beehive.BehaviorTrees;
using UnityEditor.VersionControl;
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
    public bool HasWeapon { get { return Agent.currentWeapon != null; } }

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

    public float DistanceToWeapon
    {
        get
        {
            return
                Level
                    .Instance
                    .WeaponDropTarget
                    .GetWalkingDistanceFrom(Agent.Position);
        }
    }

    public IEnumerator<TaskState> SelectWeaponTarget()
    {
        Agent.Target = Level.Instance.WeaponDropTarget;
        yield return TaskState.Success;
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

    public IEnumerator<TaskState> PickUpWeapon()
    {
        WeaponDrop[] drops = Level.Instance.garbageHome.GetComponentsInChildren<WeaponDrop>();

        TaskState taskState = TaskState.Failure;
        foreach (WeaponDrop weaponDrop in drops)
        {
            if (Vector3.Distance(weaponDrop.transform.position, Agent.Position) < 1.2f)
            {                
                Weapon instance = (Weapon)Object.Instantiate(weaponDrop.weaponPrefab, Vector3.zero, Quaternion.identity);
                instance.transform.SetParent(Agent.transform, false);
                instance.gameObject.SetActive(false);
                Agent.WeaponInstances.Add(instance);
                taskState = TaskState.Success;                
                Object.Destroy(weaponDrop.gameObject);
                break;
            }
        }
        yield return taskState;
    }

    public IEnumerator<TaskState> GoToTarget()
    {
        //Agent.Target = Level.Instance.SaviorAgentTypeTarget;
        if (Agent.Target == null)
        {
            yield return TaskState.Failure;
        }

        TaskState taskState;
        do
        {
            taskState = Agent.GoToTarget();

            if (taskState != TaskState.Running)
            {
                break;
            }
            yield return TaskState.Running;
        } while (true);

        yield return taskState;
    }

    public IEnumerator<TaskState> ClearTarget()
    {
        Agent.Target = null;
        yield return TaskState.Success;
    }

    public IEnumerator<TaskState> Wander()
    {
        Agent.Target = null;
        Agent.RotateTowards(Mathf.PerlinNoise(Agent.Position.x, Agent.Position.y + Time.time / 30) * 360);
        Agent.WantedSpeed = Vector2.zero;
        yield return TaskState.Success;
    }
}