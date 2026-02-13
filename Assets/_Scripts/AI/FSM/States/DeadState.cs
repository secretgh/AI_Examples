using UnityEngine;

public class DeadState : State
{
    private FSM_AIController npc;

    public string Name => "Dead State";

    public DeadState(FSM_AIController npc)
    {
        this.npc = npc;
    }

    public void Enter()
    {
        Debug.Log("NPC Died");
        npc.Stats.isDead = true;
    }

    public void Update() { }
    public void Exit() { }

    public bool CanEnter()
    {
        return npc.Stats.health <= 0;
    }
}
