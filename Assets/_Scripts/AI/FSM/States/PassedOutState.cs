using UnityEngine;

public class PassOutState : State
{
    float forcedSleep = GameTime.Day; //fullday
    public string Name => "Passed Out State";
    private FSM_AIController npc;

    public PassOutState(FSM_AIController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        Debug.Log("Passed Out!");
        npc.Stats.isSleeping = true;
    }

    public void Update()
    {
        forcedSleep -= Time.deltaTime;

        if (forcedSleep <= 0)
        {
            NPCStats stats = npc.Stats;
            stats.AddFatigue(-100);
            npc.FSM.ChangeState(npc.IdleState);
        }
    }

    public void Exit() {
        npc.Stats.isSleeping = false;
    }

    public bool CanEnter()
    {
        return npc.Stats.fatigue >= 100;
    }
}
