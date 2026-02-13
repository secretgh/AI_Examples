using UnityEngine;

/// <summary>
/// Retreating State - Monster flees to its lair when critically low on health.
/// Features:
/// - Triggered at low health threshold (typically 25% or less)
/// - Monster runs away from combat at high speed
/// - Heads to designated lair/nest location
/// - Will rest and heal in the lair
/// - Defensive only if cornered
/// </summary>
public class MonsterRetreatingState : HybridState
{
    private MonsterAI monster;
    private float retreatHealthThreshold = 0.25f; // Retreat at 25% health
    private bool hasReachedLair = false;

    public MonsterRetreatingState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Retreating";
        
        // Behavior tree for fleeing and healing
        behaviorTree = new RetreatBehaviorTree(monster, this);
    }

    public override int Priority => 70; // High priority, just below knockdown

    public override bool CanEnter()
    {
        // Retreat if health is critically low
        return monster.stats.healthPercent <= retreatHealthThreshold &&
               !monster.stats.isDead &&
               !monster.stats.isKnockedDown; // Can't retreat while knocked down
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] *** RETREATING TO LAIR *** Health critical!");

        hasReachedLair = false;

        // Very fast escape speed
        monster.agent.speed = monster.runSpeed * 1.4f;

        // Play flee/limp animation
        if (monster.animator != null)
        {
            monster.animator.CrossFade("Limping", 0.3f);
        }

        // Clear any committed actions
        monster.stats.isInAction = false;
        monster.stats.actionTimer = 0;

        // Set destination to lair
        if (monster.lairLocation != null)
        {
            monster.agent.SetDestination(monster.lairLocation.position);
        }
        else
        {
            Debug.LogWarning("[Monster] No lair location set! Using current position.");
        }
    }

    public override void Update()
    {
        // Check if reached lair
        if (monster.lairLocation != null && !hasReachedLair)
        {
            float distanceToLair = Vector3.Distance(
                monster.transform.position, 
                monster.lairLocation.position
            );

            if (distanceToLair < 5f)
            {
                hasReachedLair = true;
                Debug.Log("[Monster] Reached lair - beginning to rest and heal!");
            }
        }

        // Run behavior tree
        base.Update();

        // Check if healed enough to return to combat
        if (monster.stats.healthPercent >= 0.5f)
        {
            Debug.Log("[Monster] Recovered enough health - returning to hunt!");
            // State machine will transition automatically
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Restore normal speed
        monster.agent.speed = monster.runSpeed;
        
        Debug.Log("[Monster] No longer retreating!");
    }

    public bool IsInLair() => hasReachedLair;
}

/// <summary>
/// Behavior tree for retreating and healing in lair.
/// Prioritizes escape, then healing, with defensive actions if cornered.
/// </summary>
public class RetreatBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterStats stats;
    private MonsterRetreatingState retreatingState;

    public RetreatBehaviorTree(MonsterAI monster, MonsterRetreatingState retreatingState)
    {
        this.monster = monster;
        this.stats = monster.stats;
        this.retreatingState = retreatingState;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Retreat & Heal",

            // Priority 1: Rest and heal in lair
            new SequenceNode("Heal in Lair",
                new ConditionNode("In Lair?",
                    () => retreatingState.IsInLair()),
                new ActionNode("Rest and Recover", 2f,
                    onStart: () => {
                        Debug.Log("[Monster] Resting in lair...");
                        monster.agent.isStopped = true;
                        
                        // Play sleep/rest animation
                        if (monster.animator != null)
                        {
                            monster.animator.CrossFade("Resting", 0.5f);
                        }
                    },
                    onComplete: () => {
                        // Heal over time
                        float healAmount = stats.maxHealth * 0.05f; // Heal 5% per tick
                        stats.health += healAmount;
                        stats.health = Mathf.Min(stats.health, stats.maxHealth);
                        
                        Debug.Log($"[Monster] Healing... {stats.health:F0}/{stats.maxHealth}");
                        
                        // Also restore stamina
                        stats.AddStamina(20f);
                        
                        // Reduce rage while resting
                        stats.rage = Mathf.Max(0, stats.rage - 10f);
                    })
            ),

            // Priority 2: Defensive swipe if player gets too close during retreat
            new SequenceNode("Desperate Defense",
                new ConditionNode("Player Too Close During Retreat?",
                    () => !retreatingState.IsInLair() &&
                          stats.GetDistanceToTarget() <= monster.meleeRange * 0.8f &&
                          stats.stamina >= 10f &&
                          Random.value > 0.7f), // Only sometimes
                new ActionNode("Desperate Swipe", 0.1f,
                    onStart: () => {
                        Debug.Log("[Monster] Desperate defensive attack!");
                        monster.agent.isStopped = true;
                        
                        // Quick defensive attack
                        stats.CommitToAction("DesperateSwipe", 1f);
                        stats.ConsumeStamina(10f);
                    },
                    onComplete: () => {
                        monster.agent.isStopped = false;
                        
                        // Continue fleeing
                        if (monster.lairLocation != null)
                        {
                            monster.agent.SetDestination(monster.lairLocation.position);
                        }
                    })
            ),

            // Priority 3: Keep running to lair
            new SequenceNode("Flee to Lair",
                new ConditionNode("Not In Lair Yet?",
                    () => !retreatingState.IsInLair() && 
                          monster.lairLocation != null),
                new ActionNode("Sprint to Safety", 0.5f,
                    onStart: () => {
                        monster.agent.isStopped = false;
                        monster.agent.SetDestination(monster.lairLocation.position);
                        
                        // Look back occasionally (visual only)
                        if (Random.value > 0.9f)
                        {
                            Debug.Log("[Monster] Looking back while fleeing...");
                        }
                    },
                    onComplete: () => { })
            ),

            // Fallback: No lair, just back away from player
            new SequenceNode("Retreat Without Lair",
                new ConditionNode("No Lair Set?", 
                    () => monster.lairLocation == null),
                new ActionNode("Back Away from Player", 1f,
                    onStart: () => {
                        Debug.LogWarning("[Monster] No lair location! Backing away...");
                        
                        if (stats.target != null)
                        {
                            // Move away from player
                            Vector3 directionAway = (monster.transform.position - stats.target.position).normalized;
                            Vector3 retreatPosition = monster.transform.position + directionAway * 15f;
                            
                            monster.agent.SetDestination(retreatPosition);
                        }
                    },
                    onComplete: () => {
                        // Slow healing even without lair
                        stats.health += stats.maxHealth * 0.02f;
                        stats.health = Mathf.Min(stats.health, stats.maxHealth);
                    })
            )
        );
    }
}
