using UnityEngine;

/// <summary>
/// Enraged State - Monster enters a powered-up aggressive state when rage is maxed.
/// Features:
/// - Faster movement speed
/// - More aggressive attack patterns
/// - Reduced stamina costs
/// - Enhanced damage output
/// - New combo attacks
/// </summary>
public class MonsterEnragedState : HybridState
{
    private MonsterAI monster;
    private float enragedDuration = 45f; // Stays enraged for ~45 seconds
    private float enragedTimer;

    public MonsterEnragedState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Enraged";
        
        // Aggressive behavior tree for enraged combat
        behaviorTree = new EnragedBehaviorTree(monster);
    }

    public override int Priority => 40; // Higher than normal hunting

    public override bool CanEnter()
    {
        return monster.stats.isEnraged && 
               !monster.stats.isDead && 
               !monster.stats.isKnockedDown && 
               !monster.stats.isExhausted;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] *** ENRAGED *** Monster has entered a powered-up state!");

        enragedTimer = enragedDuration;

        // Increase movement speed
        monster.agent.speed = monster.runSpeed * monster.enragedSpeedMultiplier;

        // Visual effects (particles, glowing eyes, etc.)
        // PlayEnrageEffect();

        // Play enrage roar animation
        if (monster.animator != null)
        {
            monster.animator.CrossFade("EnrageRoar", 0.1f);
            monster.stats.CommitToAction("EnrageRoar", 2f); // Roar for 2 seconds
        }
    }

    public override void Update()
    {
        // Tick behavior tree
        base.Update();

        // Enrage duration countdown
        enragedTimer -= Time.deltaTime;

        // Exit enrage when timer expires
        if (enragedTimer <= 0)
        {
            monster.stats.rage = 0;
            Debug.Log("[Monster] Enrage ended - calming down...");
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Restore normal speed
        monster.agent.speed = monster.runSpeed;
        
        Debug.Log("[Monster] No longer enraged");
    }
}

/// <summary>
/// Behavior tree for enraged state.
/// More aggressive than normal hunting - constant pressure on player.
/// </summary>
public class EnragedBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterStats stats;

    public EnragedBehaviorTree(MonsterAI monster)
    {
        this.monster = monster;
        this.stats = monster.stats;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Enraged Combat",

            // Don't do anything if committed to an action
            new SequenceNode("Action Commitment Check",
                new ConditionNode("Currently In Action?", () => stats.isInAction),
                new ActionNode("Wait For Action", 0.1f,
                    onStart: () => { },
                    onComplete: () => { })
            ),

            // Priority 1: Enraged Combo Attack (close range, high aggression)
            new SequenceNode("Enraged Combo",
                new ConditionNode("In Melee Range & Has Stamina?",
                    () => stats.GetDistanceToTarget() <= monster.meleeRange && 
                          stats.stamina >= 25f &&
                          Random.value > 0.5f), // 50% chance for variety
                new ActionNode("Triple Claw Combo", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] *** ENRAGED COMBO! ***");
                        monster.SnapToTarget();
                        monster.agent.isStopped = true;
                        
                        // Commit to the combo animation
                        stats.CommitToAction("EnragedCombo", 3.5f);
                        stats.ConsumeStamina(25f);
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 2: Charging Bite (medium range)
            new SequenceNode("Enraged Charge",
                new ConditionNode("In Charge Range & Has Stamina?",
                    () => {
                        float dist = stats.GetDistanceToTarget();
                        return dist > monster.meleeRange && 
                               dist <= monster.chargeRange && 
                               stats.stamina >= 30f &&
                               stats.IsTargetInFront(45f);
                    }),
                new ActionNode("Charging Bite Attack", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] Enraged charging bite!");
                        monster.SnapToTarget();
                        
                        // Commit to charge
                        stats.CommitToAction("ChargingBite", 2.5f);
                        stats.ConsumeStamina(30f);
                        
                        // Move forward during charge (handled by animation or root motion)
                    },
                    onComplete: () => { })
            ),

            // Priority 3: Tail Spin (if player is behind)
            new SequenceNode("Enraged Tail Spin",
                new ConditionNode("Player Behind & Has Stamina?",
                    () => stats.IsTargetBehind(120f) && 
                          stats.stamina >= 20f),
                new ActionNode("360 Tail Spin", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] Enraged tail spin!");
                        
                        stats.CommitToAction("TailSpin", 2f);
                        stats.ConsumeStamina(20f);
                        monster.agent.isStopped = true;
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 4: Quick Claw Swipe (fast attack)
            new SequenceNode("Quick Swipe",
                new ConditionNode("Very Close?",
                    () => stats.GetDistanceToTarget() <= monster.meleeRange * 1.5f && 
                          stats.stamina >= 15f),
                new ActionNode("Fast Claw Swipe", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] Quick claw swipe!");
                        monster.SnapToTarget();
                        
                        stats.CommitToAction("QuickSwipe", 1.2f);
                        stats.ConsumeStamina(15f);
                        monster.agent.isStopped = true;
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                    })
            ),

            // Priority 5: Aggressive Chase
            new SequenceNode("Chase Down Player",
                new ConditionNode("Player Far?",
                    () => stats.GetDistanceToTarget() > monster.meleeRange),
                new ActionNode("Sprint Toward Player", 0.3f,
                    onStart: () => {
                        // Run at player aggressively
                        if (stats.target != null)
                        {
                            monster.agent.isStopped = false;
                            monster.agent.SetDestination(stats.target.position);
                            monster.agent.speed = monster.runSpeed * monster.enragedSpeedMultiplier;
                        }
                    },
                    onComplete: () => { })
            ),

            // Fallback: Face and prepare
            new SequenceNode("Prepare Next Attack",
                new ConditionNode("Default", () => true),
                new ActionNode("Face Target", 0.2f,
                    onStart: () => {
                        monster.FaceTarget(8f); // Fast turning while enraged
                    },
                    onComplete: () => { })
            )
        );
    }
}
