using UnityEngine;

/// <summary>
/// Dead State - Terminal state when monster health reaches 0.
/// Plays death animation and disables AI.
/// </summary>
public class MonsterDeadState : HybridState
{
    private MonsterAI monster;
    private bool deathAnimationPlayed = false;

    public MonsterDeadState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Dead";
    }

    public override int Priority => 100; // Highest priority

    public override bool CanEnter()
    {
        return monster.stats.isDead;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] Death state entered - Monster slain!");

        // Stop all movement
        monster.agent.isStopped = true;
        monster.agent.velocity = Vector3.zero;

        // Play death animation
        if (monster.animator != null && !deathAnimationPlayed)
        {
            monster.animator.CrossFade("Death", 0.2f);
            deathAnimationPlayed = true;
        }

        // Clear any committed actions
        monster.stats.isInAction = false;
        monster.stats.actionTimer = 0;

        // Optional: Disable colliders after a delay
        // Optional: Spawn loot
        // Optional: Award player rewards
    }

    public override void Update()
    {
        // Dead state does nothing
        // Could handle ragdoll physics or death animation here
    }

    public override void Exit()
    {
        // Cannot exit death state
    }
}
