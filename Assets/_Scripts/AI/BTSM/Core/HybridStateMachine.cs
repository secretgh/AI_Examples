using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Hybrid State Machine that manages state transitions with priority support.
/// States are checked in priority order, allowing critical states to override others.
/// </summary>
public class HybridStateMachine
{
    public HybridState CurrentState { get; private set; }
    private List<HybridState> allStates = new List<HybridState>();
    
    /// <summary>
    /// Register a state with the state machine
    /// </summary>
    public void RegisterState(HybridState state)
    {
        if (!allStates.Contains(state))
        {
            allStates.Add(state);
            // Sort states by priority (highest first)
            allStates = allStates.OrderByDescending(s => s.Priority).ToList();
        }
    }

    /// <summary>
    /// Register multiple states at once
    /// </summary>
    public void RegisterStates(params HybridState[] states)
    {
        foreach (var state in states)
        {
            RegisterState(state);
        }
    }

    /// <summary>
    /// Force a state change (useful for initialization or critical overrides)
    /// </summary>
    public void ChangeState(HybridState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    /// <summary>
    /// Evaluate all states and transition to the highest priority valid state
    /// </summary>
    public void EvaluateTransitions()
    {
        // Check all states in priority order
        foreach (var state in allStates)
        {
            // Skip current state
            if (state == CurrentState)
                continue;

            // If a higher priority state can be entered, transition to it
            if (state.CanEnter())
            {
                ChangeState(state);
                return;
            }
        }
    }

    /// <summary>
    /// Update the current state and check for transitions
    /// </summary>
    public void Tick()
    {
        // First evaluate if we should transition
        EvaluateTransitions();
        
        // Then update the current state
        CurrentState?.Update();
    }

    /// <summary>
    /// Get all registered states
    /// </summary>
    public IEnumerable<HybridState> GetAllStates()
    {
        return allStates;
    }

    /// <summary>
    /// Get all states that can currently be entered
    /// </summary>
    public IEnumerable<HybridState> GetValidStates()
    {
        return allStates.Where(s => s.CanEnter());
    }
}
