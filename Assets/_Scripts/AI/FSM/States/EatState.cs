using UnityEngine;

public class EatState : State
{
    float eatTimer;
    private FSM_AIController npc;
    public string Name => "Eat State";
    public EatState(FSM_AIController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        eatTimer = GameTime.Day * 0.05f; //5% of a day.
        npc.Stats.isEating = true;
        Debug.Log("Eating");
    }

    public void Update()
    {
        NPCStats stats = npc.Stats;
        eatTimer -= Time.deltaTime;

        if (eatTimer <= 0)
        {
            stats.AddFood(-1);
            stats.AddHunger(-Random.Range(30f,41f));

            npc.FSM.ChangeState(npc.IdleState);
        }
    }

    public void Exit() {
        npc.Stats.isEating = false;
    }

    public bool CanEnter()
    {
        return npc.Stats.hunger >= 40 && npc.Stats.food > 0;
    }
}
