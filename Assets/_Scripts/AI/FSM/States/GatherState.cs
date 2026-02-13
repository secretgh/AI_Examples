using UnityEngine;

public class GatherState : State
{
    float gatherTimer;
    bool reachedBush;
    private FSM_AIController npc;
    public string Name => "Gather State";
    public GatherState(FSM_AIController npc)
    {
        this.npc = npc;
    }
    public void Enter()
    {
        gatherTimer = GameTime.Day * Random.Range(0.15f, 0.26f); //Quarter day to gather. 
        reachedBush = false;
        npc.agent.SetDestination(npc.Bush.position);

        Debug.Log("Gathering Berries");
    }

    public void Update()
    {
        if (!reachedBush)
        {
            reachedBush = npc.agent.remainingDistance <= npc.agent.stoppingDistance;
            return;
        }

        gatherTimer -= Time.deltaTime;

        if (gatherTimer <= 0)
        {
            NPCStats stats = npc.Stats;
            stats.AddFood(Random.Range(1,4));

            npc.FSM.ChangeState(npc.IdleState);
        }
    }

    public void Exit() { 
        npc.agent.ResetPath();
    }

    public bool CanEnter()
    {
        return npc.Stats.hunger >= 40 && npc.Stats.food <= 0;
    }
}
