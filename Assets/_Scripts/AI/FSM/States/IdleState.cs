using UnityEngine;
public class IdleState : State
{
    private FSM_AIController npc;
    public string Name => "Idle State";
    public IdleState(FSM_AIController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        Debug.Log("Entering Idle");
    }

    public void Update()
    {
        var s = npc.Stats;

        if (s.hunger >= 40 && s.food > 0)
        {
            npc.FSM.ChangeState(npc.EatState);
        }
        else if (s.food <= 0)
        {
            npc.FSM.ChangeState(npc.GatherState);
        }
        else if (s.fatigue >= 70) { 
            npc.FSM.ChangeState(npc.SleepState);
        }
    }

    public void Exit() {}

    public bool CanEnter()
    {
        return true;
    }
}
