using UnityEngine;

/// <summary>
/// Passed Out State - High priority emergency state.
/// Entered when fatigue reaches 100. Forces the NPC to sleep for recovery.
/// Uses a simple timer, no behavior tree needed.
/// </summary>
public class HybridPassedOutState : HybridState
{
    private Hybrid_AIController controller;
    private float recoveryTimer;
    private const float RECOVERY_DURATION = GameTime.Day * 0.5f; // Half day recovery

    public HybridPassedOutState(Hybrid_AIController controller)
    {
        this.controller = controller;
        Name = "Passed Out";
    }

    public override int Priority => 50; // High priority - emergency state

    public override void Enter()
    {
        base.Enter();
        recoveryTimer = RECOVERY_DURATION;
        controller.Stats.isSleeping = true;
        controller.Stats.isEating = false;
        
        // Stop any movement
        controller.agent.ResetPath();
        controller.agent.isStopped = true;
        
        Debug.Log($"[Hybrid] NPC passed out from exhaustion! Recovering for {RECOVERY_DURATION}s");
    }

    public override void Update()
    {
        recoveryTimer -= Time.deltaTime;

        if (recoveryTimer <= 0)
        {
            // Full recovery from passing out
            controller.Stats.AddFatigue(-100);
            // State machine will automatically transition to AliveState
        }
    }

    public override void Exit()
    {
        base.Exit();
        controller.Stats.isSleeping = false;
        controller.agent.isStopped = false;
    }

    public override bool CanEnter()
    {
        return controller.Stats.fatigue >= 100 && controller.Stats.health > 0;
    }
}
