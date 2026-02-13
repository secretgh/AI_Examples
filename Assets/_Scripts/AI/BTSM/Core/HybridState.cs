using UnityEngine;

/// <summary>
/// Base class for hybrid states that combine FSM structure with Behavior Tree decision making.
/// Each state can have its own behavior tree for internal decision making.
/// </summary>
public abstract class HybridState
{
    public string Name { get; protected set; }
    protected BehaviorTree behaviorTree;
    
    /// <summary>
    /// Called when entering this state
    /// </summary>
    public virtual void Enter()
    {
        Debug.Log($"[Hybrid] Entering {Name}");
    }

    /// <summary>
    /// Called every frame while in this state
    /// </summary>
    public virtual void Update()
    {
        // Tick the behavior tree if this state has one
        behaviorTree?.Tick();
    }

    /// <summary>
    /// Called when exiting this state
    /// </summary>
    public virtual void Exit()
    {
        Debug.Log($"[Hybrid] Exiting {Name}");
    }

    /// <summary>
    /// Condition check for whether this state can be entered.
    /// Used by the state machine for transitions.
    /// </summary>
    public abstract bool CanEnter();

    /// <summary>
    /// Priority for this state. Higher priority states are checked first.
    /// Useful for critical states like Death or Emergency that should override others.
    /// Default: 0 (normal priority)
    /// Suggested: 100 for critical, 50 for high, 0 for normal, -50 for low
    /// </summary>
    public virtual int Priority => 0;
}
