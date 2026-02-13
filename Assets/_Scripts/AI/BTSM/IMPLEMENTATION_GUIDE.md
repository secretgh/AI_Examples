# Quick Implementation Guide - Unity Setup

## 📦 What You Received

**Core Reusable System** (Use in any project):
- `HybridState.cs` - Base state class
- `HybridStateMachine.cs` - State machine manager

**NPC Implementation** (Your survival AI example):
- `Hybrid_AIController.cs` - Main controller
- `HybridDeadState.cs` - Death state
- `HybridPassedOutState.cs` - Emergency fatigue state  
- `HybridAliveState.cs` - Normal state with behavior tree

**Documentation**:
- `README.md` - Full system documentation
- `COMPARISON.md` - FSM vs BT vs Hybrid comparison
- `USAGE_EXAMPLES.md` - 5 practical examples
- `IMPLEMENTATION_GUIDE.md` - This file

---

## 🚀 Unity Setup (5 Minutes)

### Step 1: Import Core Files

1. Create folder: `Assets/Scripts/AI/HybridSystem/Core/`
2. Copy these files:
   - `HybridState.cs`
   - `HybridStateMachine.cs`

### Step 2: Import NPC Implementation

1. Create folder: `Assets/Scripts/AI/HybridSystem/NPC/`
2. Copy these files:
   - `Hybrid_AIController.cs`
   - `HybridDeadState.cs`
   - `HybridPassedOutState.cs`
   - `HybridAliveState.cs`

### Step 3: Setup Your NPC

1. **In Unity:**
   - Create your NPC GameObject
   - Add `NavMeshAgent` component
   - Add `NPCStats` component
   - Add `Hybrid_AIController` component

2. **Assign References:**
   - `Stats` → NPCStats component
   - `Agent` → NavMeshAgent component
   - `Bed` → Transform of bed object
   - `Bush` → Transform of berry bush

3. **Press Play!**

That's it! Your NPC now uses the hybrid AI system.

---

## 🎮 Testing Your NPC

### In Play Mode

**Monitor States:**
```csharp
// Add this to Hybrid_AIController for debugging
void OnGUI()
{
    GUI.Label(new Rect(10, 10, 300, 20), 
        $"Current State: {StateMachine.CurrentState?.Name}");
}
```

**Test Scenarios:**

1. **Normal Life Cycle:**
   - Watch NPC gather → eat → sleep → repeat
   - Should transition smoothly

2. **Emergency States:**
   - Set `Stats.fatigue = 100` in inspector
   - Should immediately pass out

3. **Death:**
   - Set `Stats.health = 0` in inspector
   - Should immediately die (highest priority)

4. **Recovery:**
   - Click "Reset" (add a button in inspector)
   - Should return to Alive state

---

## 🔧 Customization Quick Tips

### Change Behavior Priorities

In `HybridAliveState.cs`, the selector evaluates top-to-bottom:

```csharp
new SelectorNode("Survival Priority Selector",
    // HIGHEST PRIORITY - evaluated first
    new SequenceNode("Critical Hunger Response", ...),
    
    // Add your custom behavior here
    new SequenceNode("My Custom Behavior",
        new ConditionNode("My Condition?", () => myCondition),
        new ActionNode("My Action", duration, onStart, onComplete)
    ),
    
    // LOWEST PRIORITY - evaluated last
    new SequenceNode("Idle/Rest", ...)
)
```

### Add a New State

```csharp
// 1. Create the state
public class HybridStunnedState : HybridState
{
    Hybrid_AIController controller;
    float stunDuration = 3f;
    float stunTimer;
    
    public HybridStunnedState(Hybrid_AIController controller)
    {
        this.controller = controller;
        Name = "Stunned";
    }
    
    public override int Priority => 75; // Between Dead and PassedOut
    
    public override bool CanEnter() => controller.Stats.isStunned;
    
    public override void Enter()
    {
        base.Enter();
        stunTimer = stunDuration;
        controller.agent.isStopped = true;
    }
    
    public override void Update()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
            controller.Stats.isStunned = false;
    }
    
    public override void Exit()
    {
        base.Exit();
        controller.agent.isStopped = false;
    }
}

// 2. Register in Hybrid_AIController
void Awake()
{
    // ... existing code ...
    var stunnedState = new HybridStunnedState(this);
    StateMachine.RegisterStates(DeadState, stunnedState, PassedOutState, AliveState);
}
```

---

## 🐛 Common Issues & Solutions

### NPC Doesn't Move
**Problem:** NavMeshAgent not set up
**Fix:** 
1. Bake NavMesh (Window → AI → Navigation)
2. Ensure Bed/Bush are on NavMesh
3. Check agent.stoppingDistance in inspector

### States Not Transitioning
**Problem:** CanEnter() conditions never true
**Fix:** Add debug logs:
```csharp
public override bool CanEnter()
{
    bool result = stats.hunger >= 60;
    Debug.Log($"CanEnter() = {result} (hunger: {stats.hunger})");
    return result;
}
```

### Behavior Tree Not Running
**Problem:** State doesn't have BT assigned
**Fix:** Ensure state constructor creates BT:
```csharp
public MyState(controller)
{
    behaviorTree = new MyBehaviorTree(controller); // Must do this!
}
```

---

## 🎯 What to Read Next

1. **README.md** - Complete system documentation
2. **USAGE_EXAMPLES.md** - 5 different AI implementations you can copy
3. **COMPARISON.md** - Understand why hybrid is better than FSM or BT alone

---

**You're ready! Press play and watch your hybrid AI work! 🎮**
