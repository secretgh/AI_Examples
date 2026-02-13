using UnityEngine;

/// <summary>
/// Exhausted State - Monster has run out of stamina from aggressive actions.
/// Must rest and recover stamina before returning to normal behavior.
/// Monster is vulnerable during this time with slower movement and limited attacks.
/// </summary>
public class MonsterExhaustedState : HybridState
{
    private MonsterAI monster;
    private float exhaustionTimer;
    private float minExhaustionDuration = 8f; // Must rest for at least 8 seconds
    private Vector3 retreatPosition;
    private bool hasRetreatPosition = false;

    public MonsterExhaustedState(MonsterAI monster)
    {
        this.monster = monster;
        Name = "Exhausted";
        
        // Simple behavior tree for exhausted state
        behaviorTree = new ExhaustedBehaviorTree(monster);
    }

    public override int Priority => 60; // High priority

    public override bool CanEnter()
    {
        return monster.stats.isExhausted && 
               !monster.stats.isDead && 
               !monster.stats.isKnockedDown;
    }

    public override void Enter()
    {
        base.Enter();
        
        Debug.Log("[Monster] EXHAUSTED! Must rest to recover stamina!");

        exhaustionTimer = minExhaustionDuration;

        // Slow down considerably
        monster.agent.speed = monster.walkSpeed * 0.5f;

        // Find a retreat position (away from player)
        if (monster.stats.target != null)
        {
            Vector3 directionAway = (monster.transform.position - monster.stats.target.position).normalized;
            retreatPosition = monster.transform.position + directionAway * 10f;
            hasRetreatPosition = true;
        }

        // Play exhausted animation
        if (monster.animator != null)
        {
            monster.animator.CrossFade("Exhausted", 0.3f);
        }

        // Clear any action commitment
        monster.stats.isInAction = false;
        monster.stats.actionTimer = 0;
    }

    public override void Update()
    {
        exhaustionTimer -= Time.deltaTime;

        // Run the behavior tree (handles movement and recovery)
        base.Update();

        // Check if recovered enough stamina
        if (exhaustionTimer <= 0 && monster.stats.stamina >= monster.stats.maxStamina * 0.5f)
        {
            monster.stats.isExhausted = false;
            Debug.Log("[Monster] Recovered from exhaustion!");
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        // Restore normal speed
        monster.agent.speed = monster.walkSpeed;
        
        Debug.Log("[Monster] No longer exhausted - returning to combat!");
    }

    public Vector3 GetRetreatPosition() => retreatPosition;
    public bool HasRetreatPosition() => hasRetreatPosition;
}

/// <summary>
/// Behavior tree for exhausted state.
/// Handles retreat movement and defensive actions.
/// </summary>
public class ExhaustedBehaviorTree : BehaviorTree
{
    private MonsterAI monster;
    private MonsterExhaustedState exhaustedState;

    public ExhaustedBehaviorTree(MonsterAI monster)
    {
        this.monster = monster;
        // Get reference to the state (a bit hacky, but works)
        exhaustedState = monster.StateMachine.CurrentState as MonsterExhaustedState;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        return new SelectorNode("Exhausted Behavior",
            
            // Priority 1: Retreat to safe distance
            new SequenceNode("Retreat",
                new ConditionNode("Too Close to Player?",
                    () => {
                        var state = monster.ExhaustedState;
                        return state.HasRetreatPosition() && 
                               Vector3.Distance(monster.transform.position, state.GetRetreatPosition()) > 1f;
                    }),
                new ActionNode("Move Away", 0.5f,
                    onStart: () => {
                        var state = monster.ExhaustedState;
                        if (state.HasRetreatPosition())
                        {
                            monster.agent.SetDestination(state.GetRetreatPosition());
                        }
                    },
                    onComplete: () => { })
            ),

            // Priority 2: Defensive stance and stamina recovery
            new SequenceNode("Recover Stamina",
                new ConditionNode("In Safe Position?", () => true),
                new ActionNode("Rest and Recover", 1f,
                    onStart: () => {
                        monster.agent.isStopped = true;
                        Debug.Log("[Monster] Resting to recover stamina...");
                    },
                    onComplete: () => {
                        // Stamina regenerates automatically in MonsterStats
                        // Just face the player defensively
                        monster.FaceTarget(2f);
                    })
            )
        );
    }
}
