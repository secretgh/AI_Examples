# Hybrid FSM + Behavior Tree AI System

## Overview

This hybrid system combines the strengths of **Finite State Machines (FSM)** and **Behavior Trees (BT)** to create a production-ready, extensible AI framework for game agents.

### Why Hybrid?

**FSM Strengths:**
- Clear state boundaries and transitions
- Guaranteed handling of critical conditions (death, emergencies)
- Simple to debug and visualize
- Predictable state flow

**Behavior Tree Strengths:**
- Flexible, data-driven decision making
- Easy to extend and modify
- Natural priority handling through selectors
- Reusable node composition

**Hybrid Approach:**
- FSM manages **high-level states** (Alive, Dead, PassedOut)
- BT manages **tactical decisions** within states (what to do when alive)
- Critical states use FSM for guaranteed control
- Complex behaviors use BT for flexibility

---

## Architecture

```
Hybrid_AIController
    └── HybridStateMachine
            ├── DeadState (Priority: 100)
            ├── PassedOutState (Priority: 50)
            └── AliveState (Priority: 0)
                    └── AliveBehaviorTree
                            ├── Critical Hunger Response
                            ├── High Fatigue Sleep
                            ├── Regular Eating
                            ├── Normal Sleep
                            ├── Gather Food
                            ├── Stock Up Food
                            └── Idle/Rest
```

### State Priority System

States are evaluated in priority order (highest first):
- **Priority 100**: Critical states (Death)
- **Priority 50**: Emergency states (Passed Out)
- **Priority 0**: Normal states (Alive)

This ensures critical conditions **always** override normal behavior.

---

## Core Components

### 1. HybridState (Base Class)

Abstract base class for all hybrid states. Provides:
- `Enter()` / `Update()` / `Exit()` lifecycle
- `CanEnter()` - condition for state transition
- `Priority` - determines evaluation order
- Optional `behaviorTree` for internal decision making

```csharp
public abstract class HybridState
{
    public string Name { get; protected set; }
    protected BehaviorTree behaviorTree;
    
    public virtual void Enter() { }
    public virtual void Update() { behaviorTree?.Tick(); }
    public virtual void Exit() { }
    public abstract bool CanEnter();
    public virtual int Priority => 0;
}
```

### 2. HybridStateMachine

Manages state registration, transitions, and updates:
- Automatically sorts states by priority
- Evaluates transitions every frame
- Transitions to highest priority valid state
- Updates current state's behavior tree

```csharp
public class HybridStateMachine
{
    public HybridState CurrentState { get; private set; }
    
    public void RegisterState(HybridState state) { }
    public void ChangeState(HybridState newState) { }
    public void EvaluateTransitions() { }
    public void Tick() { }
}
```

### 3. Hybrid_AIController

Main controller that:
- Inherits from `AIController` base class
- Creates and manages the state machine
- Initializes all states
- Provides `Reset()` functionality

---

## NPC Example Implementation

### States

**1. HybridDeadState (Priority: 100)**
- Terminal state - cannot exit
- Stops all movement and actions
- No behavior tree needed

**2. HybridPassedOutState (Priority: 50)**
- Emergency recovery state
- Forces half-day sleep
- Simple timer-based recovery
- No behavior tree needed

**3. HybridAliveState (Priority: 0)**
- Normal operational state
- **Uses Behavior Tree** for complex decisions
- Handles all survival needs

### Alive State Behavior Tree

The `AliveBehaviorTree` uses a **Selector** pattern with 7 prioritized behaviors:

1. **Critical Hunger Response** (hunger ≥ 80, has food)
   - Emergency eating to prevent starvation
   - Fast consumption (3% of day)

2. **High Fatigue Sleep** (fatigue ≥ 80, < 100)
   - Urgent sleep before passing out
   - Longer sleep (30% of day)

3. **Regular Eating** (hunger ≥ 60, has food)
   - Normal meal consumption
   - Standard duration (5% of day)

4. **Normal Sleep** (fatigue ≥ 70, < 80)
   - Regular sleep cycle
   - Quarter day sleep (25%)

5. **Gather Food** (food ≤ 0)
   - Emergency foraging when out of food
   - Travels to bush and gathers

6. **Stock Up Food** (food ≤ 1, not urgent)
   - Proactive gathering
   - Only when other needs are met

7. **Idle/Rest** (fallback)
   - Default behavior when all needs met
   - Small health regeneration if well-fed

---

## How to Extend the System

### Creating a New State

```csharp
public class MyCustomState : HybridState
{
    private Hybrid_AIController controller;
    
    public MyCustomState(Hybrid_AIController controller)
    {
        this.controller = controller;
        Name = "My Custom State";
        
        // Optional: Create a behavior tree for this state
        behaviorTree = new MyCustomBehaviorTree(controller);
    }
    
    public override int Priority => 25; // Set priority
    
    public override bool CanEnter()
    {
        // Define when this state can be entered
        return someCondition;
    }
    
    public override void Enter()
    {
        base.Enter();
        // State initialization
    }
    
    public override void Exit()
    {
        base.Exit();
        // State cleanup
    }
}
```

### Creating a Behavior Tree for a State

```csharp
public class MyCustomBehaviorTree : BehaviorTree
{
    private Hybrid_AIController controller;
    
    public MyCustomBehaviorTree(Hybrid_AIController controller)
    {
        this.controller = controller;
        root = BuildTree();
    }
    
    public override Node BuildTree()
    {
        return new SelectorNode("Main Selector",
            new SequenceNode("Action 1",
                new ConditionNode("Check Condition", () => condition),
                new ActionNode("Do Action", duration, onStart, onComplete)
            ),
            // Add more behaviors...
        );
    }
}
```

### Registering Your State

In `Hybrid_AIController.Awake()`:

```csharp
void Awake()
{
    StateMachine = new HybridStateMachine();
    
    // Create your states
    MyCustomState = new MyCustomState(this);
    
    // Register with state machine
    StateMachine.RegisterStates(
        DeadState,
        PassedOutState,
        MyCustomState,  // Your new state
        AliveState
    );
}
```

---

## Using This System in Other Projects

The hybrid system is designed to be **completely reusable**. Here's how:

### 1. Core Files (Reusable for Any Project)

Copy these files to any project:
- `HybridState.cs` - Base state class
- `HybridStateMachine.cs` - State machine manager

These are **completely generic** and work with any game.

### 2. Project-Specific Implementation

Create your own:
- Controller class (inherits from your base controller)
- Concrete state classes (inherit from `HybridState`)
- Behavior trees for states that need them

### Example: Enemy Combat AI

```csharp
// Your custom controller
public class EnemyCombat_AIController : MonoBehaviour
{
    public HybridStateMachine StateMachine;
    public EnemyStats stats;
    
    void Awake()
    {
        StateMachine = new HybridStateMachine();
        
        var deadState = new EnemyDeadState(this);
        var fleeState = new EnemyFleeState(this);
        var combatState = new EnemyCombatState(this);
        
        StateMachine.RegisterStates(deadState, fleeState, combatState);
    }
}

// Combat state with behavior tree
public class EnemyCombatState : HybridState
{
    public EnemyCombatState(EnemyCombat_AIController controller)
    {
        Name = "Combat";
        behaviorTree = new CombatBehaviorTree(controller);
    }
    
    public override bool CanEnter()
    {
        return controller.stats.health > 20 && controller.HasTarget();
    }
}
```

---

## Advantages Over Pure FSM or BT

### vs. Pure FSM
- ✅ More flexible decision-making within states
- ✅ Easier to add new behaviors without new states
- ✅ Better handling of priority hierarchies
- ✅ Less code duplication

### vs. Pure BT
- ✅ Guaranteed critical state handling (death, emergencies)
- ✅ Clearer high-level behavior structure
- ✅ Easier to debug state flow
- ✅ Better separation of concerns

### Hybrid Benefits
- ✅ **Best of both worlds**
- ✅ FSM ensures critical conditions never get lost
- ✅ BT provides rich, flexible decision trees
- ✅ Easy to extend and maintain
- ✅ Production-ready with priority system
- ✅ Completely reusable architecture

---

## Debug Tips

### Viewing Current State
```csharp
Debug.Log($"Current State: {StateMachine.CurrentState.Name}");
```

### Checking Valid Transitions
```csharp
foreach (var state in StateMachine.GetValidStates())
{
    Debug.Log($"Can transition to: {state.Name}");
}
```

### Behavior Tree Status
Add this to see what the BT is doing:
```csharp
public override Node BuildTree()
{
    var tree = new SelectorNode("Root",
        new ActionNode("Test",
            onStart: () => Debug.Log("BT: Starting Test Action"),
            onComplete: () => Debug.Log("BT: Completed Test Action")
        )
    );
}
```

---

## Performance Considerations

- **State evaluation**: O(n) where n = number of states (typically 3-5)
- **Priority sorting**: Done once at registration
- **BT evaluation**: Only runs for current state
- **Minimal overhead**: ~0.1ms per frame for typical use

### Optimization Tips
1. Keep number of states reasonable (3-10)
2. Use condition checks efficiently (avoid expensive calculations)
3. Cache references in state constructors
4. Use appropriate BT node types (Sequence vs Selector)

---

## Files Included

1. **Core System** (Reusable):
   - `HybridState.cs`
   - `HybridStateMachine.cs`

2. **NPC Implementation** (Example):
   - `Hybrid_AIController.cs`
   - `HybridDeadState.cs`
   - `HybridPassedOutState.cs`
   - `HybridAliveState.cs`

3. **Documentation**:
   - `README.md` (this file)

---

## Quick Start

1. Copy core files to your project
2. Create a controller class
3. Define your states (inherit from `HybridState`)
4. Register states in your controller
5. Start the state machine in `Start()`
6. Call `Tick()` in `Update()`

That's it! You now have a production-ready hybrid AI system.
