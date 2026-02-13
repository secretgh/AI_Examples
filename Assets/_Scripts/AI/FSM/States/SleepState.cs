using UnityEngine;

public class SleepState : State
{
    float sleepTimer;
    bool reachedBed;
    private FSM_AIController npc;
    public string Name => "Sleep State";
    public SleepState(FSM_AIController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        sleepTimer = GameTime.Day * 0.40f; //Sleep for 40% day ~ 8 hours
        reachedBed = false;
        npc.agent.SetDestination(npc.Bed.position);
        Debug.Log("Sleeping");
    }

    public void Update()
    {
        if (!reachedBed)
        {
            reachedBed = npc.agent.remainingDistance <= npc.agent.stoppingDistance;
            return;
        }
        else
        {
            npc.Stats.isSleeping = true;
        }

        sleepTimer -= Time.deltaTime;

        if (sleepTimer <= 0)
        {
            NPCStats stats = npc.Stats;
            stats.AddFatigue(-Random.Range(80f, 101f));
            npc.FSM.ChangeState(npc.IdleState);
        }
    }

    public void Exit() { 
        npc.Stats.isSleeping = false;
    }

    public bool CanEnter()
    {
        return npc.Stats.fatigue >= 70;
    }
}
