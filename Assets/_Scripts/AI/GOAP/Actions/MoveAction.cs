using UnityEngine;
using UnityEngine.AI;

public class MoveAction : GoapAction
{
    private NavMeshAgent agent;
    private Transform target;
    private string targetStateKey;

    public MoveAction(NavMeshAgent agent, Transform target, string targetStateKey)
    {
        Name = "MoveTo_" + target.name;
        Cost = 1f; // Can be updated dynamically based on distance

        this.agent = agent;
        this.target = target;
        this.targetStateKey = targetStateKey;

        // Moving has no preconditions other than being alive
        preconditions["Alive"] = 1;

        // Moving results in being at the target
        effects[targetStateKey] = 1;
    }

    public override void OnEnter()
    {
        agent.SetDestination(target.position);
    }

    public override bool Perform(float deltaTime)
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }
}
