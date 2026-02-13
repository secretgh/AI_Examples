using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Hybrid AI Controller that combines Finite State Machines with Behavior Trees.
/// - FSM manages high-level states (Alive, Dead, PassedOut)
/// - Behavior Trees handle complex decision-making within states
/// - Priority system ensures critical states always take precedence
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCStats))]
public class Hybrid_AIController : AIController
{
    public HybridStateMachine StateMachine { get; private set; }

    // State references
    public HybridDeadState DeadState { get; private set; }
    public HybridPassedOutState PassedOutState { get; private set; }
    public HybridAliveState AliveState { get; private set; }

    void Awake()
    {
        InitializeStateMachine();
    }

    void Start()
    {
        // Start in the Alive state
        StateMachine.ChangeState(AliveState);
    }

    void Update()
    {
        // The state machine handles transition evaluation and state updates
        StateMachine.Tick();
    }

    private void InitializeStateMachine()
    {
        StateMachine = new HybridStateMachine();

        // Initialize states (priority determines evaluation order)
        DeadState = new HybridDeadState(this);              // Priority: 100 (Critical)
        PassedOutState = new HybridPassedOutState(this);    // Priority: 50 (High)
        AliveState = new HybridAliveState(this);            // Priority: 0 (Normal)

        // Register states with the state machine
        StateMachine.RegisterStates(DeadState, PassedOutState, AliveState);
    }

    public void Reset()
    {
        // Reinitialize the state machine
        InitializeStateMachine();
        
        // Reset stats
        Stats.isDead = false;
        Stats.isSleeping = false;
        Stats.isEating = false;
        
        // Start fresh in Alive state
        StateMachine.ChangeState(AliveState);
    }
}
