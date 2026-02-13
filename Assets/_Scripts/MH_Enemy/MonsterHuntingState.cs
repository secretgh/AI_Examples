using UnityEngine;

/// <summary>
/// Hunting State - Normal combat state for the monster.
/// Features balanced attack patterns with stamina management.
/// Monster will commit to attack animations and react to player positioning.
/// </summary>
public class MonsterHuntingState : HybridState
{
    private MonsterAI monster;

    public MonsterHuntingState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Hunting";
        
        // Normal combat behavior tree
        behaviorTree = new HuntingBehaviorTree(monster);
    }

    public override int Priority => 0; // Normal priority

    public override bool CanEnter()
    {
        // Can hunt if not in any other special state
        return !monster.stats.isDead && 
               !monster.stats.isKnockedDown && 
               !monster.stats.isExhausted &&
               !monster.stats.isEnraged;
    }

    public override void Enter()
    {
        base.Enter();
        
        // Set normal movement speed
        monster.agent.speed = monster.runSpeed;
        
        Debug.Log("[Monster] Entering hunting mode - tracking target...");
    }

    public override void Update()
    {
        // Tick the behavior tree for combat decisions
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}

/// <summary>
/// Behavior tree for normal hunting/combat.
/// Manages various attacks based on range, position, and stamina.
/// Always respects action commitment - won't change behavior mid-attack.
/// </summary>
public class HuntingBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterStats stats;

    public HuntingBehaviorTree(MonsterAI monster)
    {
        this.monster = monster;
        this.stats = monster.stats;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Hunting AI",

            // CRITICAL: Check action commitment first
            // If monster is committed to an action, don't evaluate any other behaviors
            new SequenceNode("Action Commitment",
                new ConditionNode("Currently Committed to Action?", 
                    () => stats.isInAction),
                new ActionNode("Complete Current Action", 0.1f,
                    onStart: () => {
                        // Just wait - the action timer will count down in MonsterStats
                        Debug.Log($"[Monster] Committed to {stats.currentAction} ({stats.actionTimer:F1}s remaining)");
                    },
                    onComplete: () => { })
            ),

            // Priority 1: Bite Attack (close range, frontal)
            new SequenceNode("Bite Attack",
                new ConditionNode("In Bite Range?",
                    () => stats.GetDistanceToTarget() <= monster.meleeRange && 
                          stats.IsTargetInFront(60f) &&
                          stats.stamina >= 20f &&
                          Random.value > 0.6f), // 40% chance when conditions met
                new ActionNode("Execute Bite", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] BITE ATTACK!");
                        
                        // Face target
                        monster.SnapToTarget();
                        
                        // Stop movement during attack
                        monster.agent.isStopped = true;
                        
                        // Commit to bite animation (2 seconds)
                        stats.CommitToAction("Bite", 2f);
                        
                        // Consume stamina
                        stats.ConsumeStamina(20f);
                    },
                    onComplete: () => {
                        // Re-enable movement after attack
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 2: Claw Swipe (close range, wide arc)
            new SequenceNode("Claw Swipe",
                new ConditionNode("In Swipe Range?",
                    () => stats.GetDistanceToTarget() <= monster.meleeRange * 1.2f && 
                          stats.stamina >= 15f &&
                          Random.value > 0.5f),
                new ActionNode("Execute Claw Swipe", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] CLAW SWIPE!");
                        
                        monster.SnapToTarget();
                        monster.agent.isStopped = true;
                        
                        // Swipe is faster than bite
                        stats.CommitToAction("ClawSwipe", 1.5f);
                        stats.ConsumeStamina(15f);
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 3: Tail Attack (player behind)
            new SequenceNode("Tail Attack",
                new ConditionNode("Player Behind?",
                    () => stats.IsTargetBehind(120f) && 
                          stats.stamina >= 18f),
                new ActionNode("Execute Tail Whip", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] TAIL WHIP!");
                        
                        // Don't need to turn for tail attack
                        monster.agent.isStopped = true;
                        
                        stats.CommitToAction("TailWhip", 1.8f);
                        stats.ConsumeStamina(18f);
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 4: Charge Attack (medium range)
            new SequenceNode("Charge Attack",
                new ConditionNode("In Charge Range?",
                    () => {
                        float dist = stats.GetDistanceToTarget();
                        return dist > monster.meleeRange && 
                               dist <= monster.chargeRange && 
                               stats.IsTargetInFront(45f) &&
                               stats.stamina >= 35f &&
                               Random.value > 0.7f; // Less frequent, more deliberate
                    }),
                new ActionNode("Execute Charge", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] CHARGING!");
                        
                        monster.SnapToTarget();
                        
                        // Charge is a big commitment
                        stats.CommitToAction("Charge", 3f);
                        stats.ConsumeStamina(35f);
                        
                        // Charge forward (would use root motion or manual movement)
                        // For now, just the commitment
                    },
                    onComplete: () => { })
            ),

            // Priority 5: Roar (intimidation, creates opening)
            new SequenceNode("Intimidating Roar",
                new ConditionNode("Should Roar?",
                    () => stats.stamina >= 10f &&
                          stats.GetDistanceToTarget() <= monster.chargeRange &&
                          Random.value > 0.85f), // Rare, dramatic moment
                new ActionNode("Execute Roar", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] *ROAAAAAR*!");
                        
                        monster.agent.isStopped = true;
                        
                        // Roar stuns nearby players (implement in game logic)
                        stats.CommitToAction("Roar", 2.5f);
                        stats.ConsumeStamina(10f);
                        
                        // Add a bit of rage from roaring
                        stats.AddRage(10f);
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 6: Reposition (circle player)
            new SequenceNode("Reposition",
                new ConditionNode("Need Better Position?",
                    () => {
                        float dist = stats.GetDistanceToTarget();
                        // Reposition if too far or too close without stamina
                        return (dist > monster.chargeRange || 
                                (dist < monster.meleeRange * 0.5f && stats.stamina < 15f)) &&
                               Random.value > 0.6f;
                    }),
                new ActionNode("Circle Player", 1.5f,
                    onStart: () => {
                        Debug.Log("[Monster] Repositioning...");
                        
                        if (stats.target != null)
                        {
                            // Calculate a position to the side of the player
                            Vector3 toPlayer = (stats.target.position - monster.transform.position).normalized;
                            Vector3 right = Vector3.Cross(Vector3.up, toPlayer);
                            
                            // Circle left or right randomly
                            float direction = Random.value > 0.5f ? 1f : -1f;
                            Vector3 circlePosition = stats.target.position + right * direction * monster.meleeRange * 1.5f;
                            
                            monster.agent.SetDestination(circlePosition);
                            monster.agent.speed = monster.walkSpeed; // Move deliberately
                        }
                    },
                    onComplete: () => {
                        monster.agent.speed = monster.runSpeed;
                    })
            ),

            // Priority 7: Close Distance (chase)
            new SequenceNode("Chase Player",
                new ConditionNode("Player Too Far?",
                    () => stats.GetDistanceToTarget() > monster.meleeRange),
                new ActionNode("Move Toward Player", 0.5f,
                    onStart: () => {
                        if (stats.target != null)
                        {
                            monster.agent.SetDestination(stats.target.position);
                            monster.agent.speed = monster.runSpeed;
                            
                            // Face while moving
                            monster.FaceTarget(3f);
                        }
                    },
                    onComplete: () => { })
            ),

            // Priority 8: Maintain Pressure (close range, low stamina)
            new SequenceNode("Pressure Player",
                new ConditionNode("Close But Low Stamina?",
                    () => stats.GetDistanceToTarget() <= monster.meleeRange && 
                          stats.stamina < 20f),
                new ActionNode("Threatening Stance", 1f,
                    onStart: () => {
                        Debug.Log("[Monster] Low stamina - maintaining pressure...");
                        
                        monster.agent.isStopped = true;
                        monster.FaceTarget(5f);
                        
                        // Small stamina regen while threatening
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Fallback: Observe and prepare
            new SequenceNode("Observe",
                new ConditionNode("Default Behavior", () => true),
                new ActionNode("Watch Target", 0.5f,
                    onStart: () => {
                        monster.FaceTarget(2f);
                    },
                    onComplete: () => { })
            )
        );
    }
}
