using UnityEngine;

/// <summary>
/// Dead State - Highest priority critical state.
/// Once entered, the NPC cannot leave this state.
/// No behavior tree needed as this is a terminal state.
/// </summary>
public class HybridDeadState : HybridState
{
    private Hybrid_AIController controller;

    public HybridDeadState(Hybrid_AIController controller)
    {
        this.controller = controller;
        Name = "Dead";
    }

    public override int Priority => 100; // Critical priority - always checked first

    public override void Enter()
    {
        base.Enter();
        controller.Stats.isDead = true;
        controller.Stats.isSleeping = false;
        controller.Stats.isEating = false;
        
        // Stop any movement
        controller.agent.ResetPath();
        controller.agent.isStopped = true;
    }

    public override void Update()
    {
        // Dead state does nothing - terminal state
    }

    public override bool CanEnter()
    {
        return controller.Stats.health <= 0;
    }

    public override void Exit()
    {
        // Dead state cannot be exited (unless Reset is called on controller)
    }
}
