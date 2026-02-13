using UnityEngine;

/// <summary>
/// Relocating State - Monster decides to change combat location.
/// Triggered by:
/// - Taking significant damage (health threshold)
/// - Fighting too long in one area (time threshold)
/// - Tactical repositioning to more favorable terrain
/// 
/// Monster will run to a new area, then return to combat.
/// </summary>
public class MonsterRelocatingState : HybridState
{
    private MonsterAI monster;
    private Vector3 relocationTarget;
    private bool hasRelocationTarget = false;
    private float relocationDistance = 30f; // How far to relocate
    private float timeInFight = 0f;
    private float maxTimeBeforeRelocate = 120f; // Relocate after 2 minutes of fighting
    private float lastHealthCheck;
    private float healthLossThreshold = 200f; // Relocate after losing this much health

    public MonsterRelocatingState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Relocating";
        
        lastHealthCheck = monster.stats.health;
        
        // Simple behavior tree for relocation
        behaviorTree = new RelocationBehaviorTree(monster, this);
    }

    public override int Priority => 35; // Between Hunting and Enraged

    public override bool CanEnter()
    {
        // Don't relocate if in critical states
        if (monster.stats.isDead || 
            monster.stats.isKnockedDown || 
            monster.stats.isExhausted)
            return false;

        // Don't relocate if wandering (not in combat)
        if (!monster.stats.hasBeenAttacked)
            return false;

        // Check health loss trigger
        float healthLost = lastHealthCheck - monster.stats.health;
        if (healthLost >= healthLossThreshold)
        {
            Debug.Log($"[Monster] Health loss trigger! Lost {healthLost} HP");
            return true;
        }

        // Check time in fight trigger
        if (timeInFight >= maxTimeBeforeRelocate)
        {
            Debug.Log($"[Monster] Time trigger! Fought for {timeInFight}s");
            return true;
        }

        return false;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] Relocating to new area!");

        // Pick a new location
        PickRelocationTarget();

        // Fast movement during relocation
        monster.agent.speed = monster.runSpeed * 1.2f;

        // Play roar/warning animation
        if (monster.animator != null)
        {
            monster.animator.CrossFade("Roar", 0.2f);
        }

        // Reset health check
        lastHealthCheck = monster.stats.health;
        
        // Reset fight timer
        timeInFight = 0f;
    }

    public override void Update()
    {
        // Track time in combat (for next relocation)
        if (monster.stats.hasBeenAttacked && !monster.stats.isDead)
        {
            timeInFight += Time.deltaTime;
        }

        // Run behavior tree to handle movement
        base.Update();

        // Check if reached destination
        if (hasRelocationTarget && 
            Vector3.Distance(monster.transform.position, relocationTarget) < 3f)
        {
            Debug.Log("[Monster] Reached new location - resuming combat!");
            hasRelocationTarget = false;
            
            // Add some rage from relocating (monster is frustrated)
            monster.stats.AddRage(20f);
            
            // State machine will transition back to appropriate combat state
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Restore normal speed
        monster.agent.speed = monster.runSpeed;
        
        Debug.Log("[Monster] Relocation complete!");
    }

    private void PickRelocationTarget()
    {
        // Try to find a point far from current location
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 direction = new Vector3(randomDirection.x, 0, randomDirection.y);
        
        relocationTarget = monster.transform.position + direction * relocationDistance;
        hasRelocationTarget = true;

        Debug.Log($"[Monster] Target location: {relocationTarget}");
    }

    public Vector3 GetRelocationTarget() => relocationTarget;
    public bool HasRelocationTarget() => hasRelocationTarget;
}

/// <summary>
/// Behavior tree for relocation movement.
/// Runs toward new location while occasionally checking for player.
/// </summary>
public class RelocationBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterRelocatingState relocatingState;

    public RelocationBehaviorTree(MonsterAI monster, MonsterRelocatingState relocatingState)
    {
        this.monster = monster;
        this.relocatingState = relocatingState;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Relocation Behavior",

            // Priority 1: Run to new location
            new SequenceNode("Sprint to New Area",
                new ConditionNode("Has Target?",
                    () => relocatingState.HasRelocationTarget()),
                new ActionNode("Run", 0.5f,
                    onStart: () => {
                        monster.agent.isStopped = false;
                        monster.agent.SetDestination(relocatingState.GetRelocationTarget());
                    },
                    onComplete: () => {
                        // Keep running toward target
                    })
            ),

            // Fallback: Wait
            new SequenceNode("Wait",
                new ConditionNode("Default", () => true),
                new ActionNode("Idle", 0.5f,
                    onStart: () => {
                        monster.agent.isStopped = true;
                    },
                    onComplete: () => { })
            )
        );
    }
}
