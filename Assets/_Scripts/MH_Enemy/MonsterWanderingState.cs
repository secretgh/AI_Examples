using UnityEngine;

/// <summary>
/// Wandering State - Passive state when monster hasn't been aggroed yet.
/// Monster patrols its territory, occasionally stopping to idle.
/// Transitions to Hunting when player attacks or gets too close.
/// </summary>
public class MonsterWanderingState : HybridState
{
    private MonsterAI monster;
    private float aggroRange = 20f; // Distance at which monster notices player
    private float minTimeBetweenWanders = 3f;
    private float maxTimeBetweenWanders = 8f;

    public MonsterWanderingState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Wandering";
        
        // Behavior tree for wandering
        behaviorTree = new WanderingBehaviorTree(monster, this);
    }

    public override int Priority => -10; // Lower than hunting (peaceful state)

    public override bool CanEnter()
    {
        // Can wander if:
        // - Not in combat (hasn't been attacked)
        // - Not dead, knocked down, exhausted, or enraged
        // - Player hasn't entered aggro range
        return !monster.stats.hasBeenAttacked &&
               !monster.stats.isDead &&
               !monster.stats.isKnockedDown &&
               !monster.stats.isExhausted &&
               !monster.stats.isEnraged &&
               !IsPlayerInAggroRange();
    }

    public override void Enter()
    {
        base.Enter();
        
        // Slow, peaceful movement
        monster.agent.speed = monster.walkSpeed * 0.7f;
        
        Debug.Log("[Monster] Wandering peacefully in territory...");
    }

    public override void Update()
    {
        // Check if player entered aggro range or attacked
        if (IsPlayerInAggroRange() || monster.stats.hasBeenAttacked)
        {
            Debug.Log("[Monster] Player detected! Engaging combat!");
            monster.stats.hasBeenAttacked = true; // Mark as aggroed
            // State machine will transition to hunting automatically
            return;
        }

        // Tick wandering behavior tree
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        
        // Restore normal speed
        monster.agent.speed = monster.runSpeed;
    }

    private bool IsPlayerInAggroRange()
    {
        if (monster.stats.target == null) return false;
        
        float distanceToPlayer = Vector3.Distance(
            monster.transform.position, 
            monster.stats.target.position
        );
        
        return distanceToPlayer <= aggroRange;
    }

    public float GetMinWanderTime() => minTimeBetweenWanders;
    public float GetMaxWanderTime() => maxTimeBetweenWanders;
}

/// <summary>
/// Behavior tree for wandering state.
/// Handles random patrol and idle behaviors.
/// </summary>
public class WanderingBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterWanderingState wanderState;
    private Vector3 currentWanderTarget;
    private float idleTimer = 0f;
    private bool isIdling = false;

    public WanderingBehaviorTree(MonsterAI monster, MonsterWanderingState wanderState)
    {
        this.monster = monster;
        this.wanderState = wanderState;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Wander Behavior",

            // Priority 1: Idle for a bit
            new SequenceNode("Idle Period",
                new ConditionNode("Should Idle?",
                    () => isIdling && idleTimer > 0),
                new ActionNode("Stand and Observe", 1f,
                    onStart: () => {
                        monster.agent.isStopped = true;
                        idleTimer -= 1f;
                    },
                    onComplete: () => {
                        if (idleTimer <= 0)
                        {
                            isIdling = false;
                        }
                    })
            ),

            // Priority 2: Move to wander point
            new SequenceNode("Wander Movement",
                new ConditionNode("Has Wander Target?",
                    () => currentWanderTarget != Vector3.zero &&
                          Vector3.Distance(monster.transform.position, currentWanderTarget) > 1f),
                new ActionNode("Walk to Point", 0.5f,
                    onStart: () => {
                        monster.agent.isStopped = false;
                        monster.agent.SetDestination(currentWanderTarget);
                    },
                    onComplete: () => { })
            ),

            // Priority 3: Pick new wander point and idle
            new SequenceNode("Choose New Destination",
                new ConditionNode("Need New Target?", () => true),
                new ActionNode("Pick Random Point", 0.1f,
                    onStart: () => {
                        // Pick random point in territory
                        Vector2 randomCircle = Random.insideUnitCircle * 15f;
                        currentWanderTarget = monster.transform.position + 
                            new Vector3(randomCircle.x, 0, randomCircle.y);
                        
                        Debug.Log("[Monster] Wandering to new location...");
                    },
                    onComplete: () => {
                        // After reaching destination, idle for a bit
                        isIdling = true;
                        idleTimer = Random.Range(
                            wanderState.GetMinWanderTime(), 
                            wanderState.GetMaxWanderTime()
                        );
                    })
            )
        );
    }
}
