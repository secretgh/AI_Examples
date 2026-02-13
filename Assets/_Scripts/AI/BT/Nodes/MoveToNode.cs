using UnityEngine;
using UnityEngine.AI;
public class MoveToNode : Node
{
    NavMeshAgent agent;
    Transform target;
    float stoppingDistance;
    bool started;

    public MoveToNode(
        string name,
        NavMeshAgent agent,
        Transform target,
        float stoppingDistance = 0.5f) : base(name)
    {
        this.agent = agent;
        this.target = target;
        this.stoppingDistance = stoppingDistance;
    }

    public override NodeStatus Tick()
    {
        if (agent == null || target == null) { 
            status = NodeStatus.Failure;
            return NodeStatus.Failure;
        }

        if (!started)
        {
            started = true;
            agent.stoppingDistance = stoppingDistance;
            agent.SetDestination(target.position);
        }

        if (agent.pathPending) { 
            status = NodeStatus.Running;
            return NodeStatus.Running;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            status = NodeStatus.Failure;
            return NodeStatus.Failure;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            started = false;
            agent.ResetPath();
            status = NodeStatus.Success;
            return NodeStatus.Success;
        }

        status = NodeStatus.Running;
        return NodeStatus.Running;
    }
}
