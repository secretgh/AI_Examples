# Hybrid AI System - Usage Examples

This document provides practical, copy-paste examples for common use cases.

---

## Example 1: Basic Enemy Combat AI

```csharp
using UnityEngine;
using UnityEngine.AI;

// 1. Create your controller
public class EnemyAI : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public NavMeshAgent agent;
    public Transform target; // Player
    public EnemyStats stats;
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        // Create states
        var deadState = new EnemyDeadState(this);
        var fleeState = new EnemyFleeState(this);
        var combatState = new EnemyCombatState(this);
        
        // Register them
        StateMachine.RegisterStates(deadState, fleeState, combatState);
        StateMachine.ChangeState(combatState);
    }
    
    void Update()
    {
        StateMachine.Tick();
    }
}

// 2. Create Dead State (Critical)
public class EnemyDeadState : HybridState
{
    EnemyAI enemy;
    
    public EnemyDeadState(EnemyAI enemy)
    {
        this.enemy = enemy;
        Name = "Dead";
    }
    
    public override int Priority => 100;
    
    public override bool CanEnter() => enemy.stats.health <= 0;
    
    public override void Enter()
    {
        base.Enter();
        enemy.agent.isStopped = true;
        // Play death animation
        // Disable collider
        // Award XP to player
    }
}

// 3. Create Flee State (High Priority)
public class EnemyFleeState : HybridState
{
    EnemyAI enemy;
    
    public EnemyFleeState(EnemyAI enemy)
    {
        this.enemy = enemy;
        Name = "Fleeing";
    }
    
    public override int Priority => 50;
    
    public override bool CanEnter()
    {
        return enemy.stats.health > 0 && 
               enemy.stats.health < 20; // Low health
    }
    
    public override void Update()
    {
        // Run away from player
        Vector3 fleeDirection = enemy.transform.position - enemy.target.position;
        Vector3 fleeTarget = enemy.transform.position + fleeDirection.normalized * 10f;
        enemy.agent.SetDestination(fleeTarget);
    }
    
    public override void Exit()
    {
        base.Exit();
        enemy.agent.ResetPath();
    }
}

// 4. Create Combat State with Behavior Tree
public class EnemyCombatState : HybridState
{
    EnemyAI enemy;
    
    public EnemyCombatState(EnemyAI enemy)
    {
        this.enemy = enemy;
        Name = "Combat";
        behaviorTree = new CombatBehaviorTree(enemy);
    }
    
    public override int Priority => 0;
    
    public override bool CanEnter()
    {
        return enemy.stats.health >= 20; // Not fleeing
    }
}

// 5. Combat Behavior Tree
public class CombatBehaviorTree : BehaviorTree
{
    EnemyAI enemy;
    
    public CombatBehaviorTree(EnemyAI enemy)
    {
        this.enemy = enemy;
        root = BuildTree();
    }
    
    public override Node BuildTree()
    {
        float attackRange = 2f;
        float chaseRange = 15f;
        
        return new SelectorNode("Combat AI",
            // Priority 1: Attack if in range
            new SequenceNode("Attack",
                new ConditionNode("In Attack Range?",
                    () => Vector3.Distance(enemy.transform.position, enemy.target.position) <= attackRange),
                new ActionNode("Perform Attack", 1.5f,
                    onStart: () => {
                        enemy.agent.isStopped = true;
                        // Play attack animation
                    },
                    onComplete: () => {
                        // Deal damage
                        enemy.target.GetComponent<PlayerHealth>()?.TakeDamage(10);
                        enemy.agent.isStopped = false;
                    })
            ),
            
            // Priority 2: Chase if player visible
            new SequenceNode("Chase",
                new ConditionNode("Player in Range?",
                    () => Vector3.Distance(enemy.transform.position, enemy.target.position) <= chaseRange),
                new ActionNode("Move to Player", 0.1f,
                    onStart: () => {
                        enemy.agent.SetDestination(enemy.target.position);
                    },
                    onComplete: () => { })
            ),
            
            // Priority 3: Patrol
            new SequenceNode("Patrol",
                new ConditionNode("Should Patrol?", () => true),
                new ActionNode("Patrol Area", 3f,
                    onStart: () => {
                        // Random patrol point
                        Vector3 randomPoint = enemy.transform.position + Random.insideUnitSphere * 10f;
                        enemy.agent.SetDestination(randomPoint);
                    },
                    onComplete: () => { })
            )
        );
    }
}
```

---

## Example 2: NPC Daily Routine AI

```csharp
// Shop keeper with daily schedule
public class ShopKeeperAI : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public Transform shopCounter;
    public Transform home;
    public float currentTime; // 0-24 hours
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        var sleepingState = new SleepingState(this);
        var workingState = new WorkingState(this);
        var closingState = new ClosingState(this);
        
        StateMachine.RegisterStates(sleepingState, workingState, closingState);
    }
}

public class WorkingState : HybridState
{
    ShopKeeperAI npc;
    
    public WorkingState(ShopKeeperAI npc)
    {
        this.npc = npc;
        Name = "Working";
        behaviorTree = new ShopBehaviorTree(npc);
    }
    
    public override bool CanEnter()
    {
        return npc.currentTime >= 8f && npc.currentTime < 18f; // 8am - 6pm
    }
}

public class ShopBehaviorTree : BehaviorTree
{
    ShopKeeperAI npc;
    
    public ShopBehaviorTree(ShopKeeperAI npc)
    {
        this.npc = npc;
        root = BuildTree();
    }
    
    public override Node BuildTree()
    {
        return new SelectorNode("Shop Routine",
            // Customer nearby
            new SequenceNode("Serve Customer",
                new ConditionNode("Customer Present?",
                    () => Physics.CheckSphere(npc.shopCounter.position, 3f, LayerMask.GetMask("Player"))),
                new ActionNode("Greet Customer", 2f,
                    onStart: () => Debug.Log("Welcome to my shop!"),
                    onComplete: () => { /* Open shop UI */ })
            ),
            
            // No customers, organize
            new SequenceNode("Organize Shop",
                new ConditionNode("Shop Open?", () => true),
                new ActionNode("Organize Items", 5f,
                    onStart: () => Debug.Log("*organizing shelves*"),
                    onComplete: () => { })
            )
        );
    }
}
```

---

## Example 3: Boss AI with Phases

```csharp
public class BossAI : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public BossStats stats;
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        var deathState = new BossDeathState(this);
        var enragedState = new BossEnragedState(this);
        var phase2State = new BossPhase2State(this);
        var phase1State = new BossPhase1State(this);
        
        StateMachine.RegisterStates(deathState, enragedState, phase2State, phase1State);
    }
}

public class BossPhase1State : HybridState
{
    BossAI boss;
    
    public BossPhase1State(BossAI boss)
    {
        this.boss = boss;
        Name = "Phase 1";
        behaviorTree = new Phase1BehaviorTree(boss);
    }
    
    public override int Priority => 0;
    
    public override bool CanEnter() => boss.stats.healthPercent > 50;
}

public class BossPhase2State : HybridState
{
    BossAI boss;
    
    public BossPhase2State(BossAI boss)
    {
        this.boss = boss;
        Name = "Phase 2";
        behaviorTree = new Phase2BehaviorTree(boss);
    }
    
    public override int Priority => 25; // Higher than Phase 1
    
    public override bool CanEnter()
    {
        return boss.stats.healthPercent <= 50 && 
               boss.stats.healthPercent > 0;
    }
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("BOSS ENRAGES!");
        // Play phase transition cutscene
        // Change appearance
        boss.stats.damageMultiplier = 1.5f;
    }
}

public class BossEnragedState : HybridState
{
    BossAI boss;
    float rageDuration = 10f;
    float rageTimer;
    
    public BossEnragedState(BossAI boss)
    {
        this.boss = boss;
        Name = "Enraged";
    }
    
    public override int Priority => 50; // Higher than phases
    
    public override bool CanEnter()
    {
        return boss.stats.wasHitRecently && 
               boss.stats.healthPercent < 25;
    }
    
    public override void Enter()
    {
        base.Enter();
        rageTimer = rageDuration;
        boss.stats.attackSpeed *= 2f;
        // Particle effects, screen shake
    }
    
    public override void Update()
    {
        rageTimer -= Time.deltaTime;
        // Aggressive attacks
        if (rageTimer <= 0)
        {
            boss.stats.wasHitRecently = false; // Exit condition
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        boss.stats.attackSpeed /= 2f;
    }
}
```

---

## Example 4: Companion AI

```csharp
public class CompanionAI : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public Transform player;
    public CompanionStats stats;
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        var downedState = new CompanionDownedState(this);
        var combatState = new CompanionCombatState(this);
        var followState = new CompanionFollowState(this);
        
        StateMachine.RegisterStates(downedState, combatState, followState);
    }
}

public class CompanionFollowState : HybridState
{
    CompanionAI companion;
    
    public CompanionFollowState(CompanionAI companion)
    {
        this.companion = companion;
        Name = "Following";
        behaviorTree = new FollowBehaviorTree(companion);
    }
    
    public override bool CanEnter()
    {
        return companion.stats.health > 0 && 
               !companion.stats.enemiesNearby;
    }
}

public class FollowBehaviorTree : BehaviorTree
{
    CompanionAI companion;
    
    public FollowBehaviorTree(CompanionAI companion)
    {
        this.companion = companion;
        root = BuildTree();
    }
    
    public override Node BuildTree()
    {
        return new SelectorNode("Follow Player",
            // Too far, catch up
            new SequenceNode("Catch Up",
                new ConditionNode("Far from Player?",
                    () => Vector3.Distance(companion.transform.position, companion.player.position) > 10f),
                new ActionNode("Sprint to Player", 0.5f,
                    onStart: () => {
                        companion.GetComponent<NavMeshAgent>().speed = 8f;
                        companion.GetComponent<NavMeshAgent>().SetDestination(companion.player.position);
                    },
                    onComplete: () => { })
            ),
            
            // Close to player, maintain distance
            new SequenceNode("Maintain Distance",
                new ConditionNode("Near Player?", () => true),
                new ActionNode("Walk Behind Player", 1f,
                    onStart: () => {
                        companion.GetComponent<NavMeshAgent>().speed = 3.5f;
                        Vector3 behindPlayer = companion.player.position - companion.player.forward * 3f;
                        companion.GetComponent<NavMeshAgent>().SetDestination(behindPlayer);
                    },
                    onComplete: () => { })
            )
        );
    }
}
```

---

## Example 5: Stealth Guard AI

```csharp
public class GuardAI : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public GuardStats stats;
    public Transform[] patrolPoints;
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        var alertState = new GuardAlertState(this);      // Priority: 50
        var investigateState = new GuardInvestigateState(this); // Priority: 25
        var patrolState = new GuardPatrolState(this);     // Priority: 0
        
        StateMachine.RegisterStates(alertState, investigateState, patrolState);
    }
}

public class GuardPatrolState : HybridState
{
    GuardAI guard;
    int currentPatrolIndex = 0;
    
    public GuardPatrolState(GuardAI guard)
    {
        this.guard = guard;
        Name = "Patrolling";
    }
    
    public override int Priority => 0;
    
    public override bool CanEnter() => guard.stats.alertLevel == 0;
    
    public override void Update()
    {
        if (!guard.GetComponent<NavMeshAgent>().hasPath)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % guard.patrolPoints.Length;
            guard.GetComponent<NavMeshAgent>().SetDestination(guard.patrolPoints[currentPatrolIndex].position);
        }
    }
}

public class GuardInvestigateState : HybridState
{
    GuardAI guard;
    Vector3 investigatePosition;
    
    public GuardInvestigateState(GuardAI guard)
    {
        this.guard = guard;
        Name = "Investigating";
    }
    
    public override int Priority => 25;
    
    public override bool CanEnter()
    {
        return guard.stats.alertLevel > 0 && guard.stats.alertLevel < 100;
    }
    
    public override void Enter()
    {
        base.Enter();
        investigatePosition = guard.stats.lastNoisePosition;
        guard.GetComponent<NavMeshAgent>().SetDestination(investigatePosition);
    }
    
    public override void Update()
    {
        if (Vector3.Distance(guard.transform.position, investigatePosition) < 1f)
        {
            // Look around
            guard.stats.alertLevel -= Time.deltaTime * 10f; // Calm down over time
        }
    }
}

public class GuardAlertState : HybridState
{
    GuardAI guard;
    
    public GuardAlertState(GuardAI guard)
    {
        this.guard = guard;
        Name = "Alert";
        behaviorTree = new AlertBehaviorTree(guard);
    }
    
    public override int Priority => 50;
    
    public override bool CanEnter() => guard.stats.alertLevel >= 100;
    
    public override void Enter()
    {
        base.Enter();
        Debug.Log("INTRUDER ALERT!");
        // Sound alarm
        // Call for backup
    }
}
```

---

## Tips for Your Own Implementations

### 1. **Priority Guidelines**
```
100+ : Terminal/Critical (Death, Game Over)
75-99: Emergency (Stunned, Fleeing, Downed)
50-74: High Priority (Alert, Enraged, Phase Changes)
25-49: Medium Priority (Investigating, Transitioning)
0-24 : Normal Priority (Default behaviors)
-1 to -50: Low Priority (Idle, Waiting)
```

### 2. **When to Use Behavior Trees in States**
```
✅ Use BT when state has:
- Multiple possible actions
- Complex decision hierarchies
- Priority-based selection
- Example: CombatState, AliveState, PatrolState

❌ Don't use BT when state:
- Has single simple action
- Is terminal/passive
- Has fixed duration
- Example: DeadState, StunnedState, CutsceneState
```

### 3. **CanEnter() Best Practices**
```csharp
// ✅ GOOD - Simple, fast checks
public override bool CanEnter()
{
    return health <= 0;
}

// ✅ GOOD - Compound conditions
public override bool CanEnter()
{
    return health > 0 && 
           alertLevel >= 100 && 
           hasWeapon;
}

// ❌ BAD - Expensive operations
public override bool CanEnter()
{
    return Physics.CheckSphere(...) && // Expensive!
           PathExists(...); // Expensive!
}

// ✅ BETTER - Cache expensive checks
void Update()
{
    cachedEnemyNearby = Physics.CheckSphere(...);
}

public override bool CanEnter()
{
    return cachedEnemyNearby; // Fast!
}
```

---

## Common Patterns

### Pattern 1: Timed State
```csharp
public class TimedState : HybridState
{
    float duration;
    float timer;
    
    public override void Enter()
    {
        timer = duration;
    }
    
    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            // State machine will auto-transition
            // when CanEnter() returns false
        }
    }
    
    public override bool CanEnter()
    {
        return timer > 0;
    }
}
```

### Pattern 2: Health-Gated State
```csharp
public class PhaseState : HybridState
{
    float minHealth;
    float maxHealth;
    
    public override bool CanEnter()
    {
        float healthPercent = stats.health / stats.maxHealth;
        return healthPercent >= minHealth && healthPercent <= maxHealth;
    }
}
```

### Pattern 3: Cooldown State
```csharp
public class AbilityState : HybridState
{
    float cooldown = 10f;
    float lastUsed = -100f;
    
    public override bool CanEnter()
    {
        return Time.time - lastUsed >= cooldown;
    }
    
    public override void Enter()
    {
        lastUsed = Time.time;
    }
}
```

These examples show real-world applications of the hybrid system. Mix and match patterns to create your own unique AI behaviors!
