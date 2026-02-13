using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCStats))]
public class FSM_AIController : AIController
{
    public StateMachine FSM { get; private set; }

    // State references
    public IdleState IdleState;
    public EatState EatState;
    public GatherState GatherState;
    public SleepState SleepState;
    public PassOutState PassedOutState;
    public DeadState DeadState;

    void Awake()
    {
        FSM = new StateMachine();

        IdleState = new IdleState(this);
        EatState = new EatState(this);
        GatherState = new GatherState(this);
        SleepState = new SleepState(this);
        PassedOutState = new PassOutState(this);
        DeadState = new DeadState(this);

        List<State> AllStates = new List<State> { IdleState, EatState, GatherState, SleepState, PassedOutState, DeadState };
        FSM.AllStates = AllStates;
    }

    void Start()
    {
        FSM.ChangeState(IdleState);
    }

    void Update()
    {
        CheckCriticalConditions();
        FSM.Tick();
    }

    void CheckCriticalConditions()
    {
        if (Stats.health <= 0)
        {
            FSM.ChangeState(DeadState);
            return;
        }

        if (Stats.fatigue >= 100 && !(FSM.CurrentState is PassOutState))
        {
            FSM.ChangeState(PassedOutState);
            return;
        }
    }

    public void Reset()
    {
        FSM = new StateMachine();
        IdleState = new IdleState(this);
        EatState = new EatState(this);
        GatherState = new GatherState(this);
        SleepState = new SleepState(this);
        PassedOutState = new PassOutState(this);
        DeadState = new DeadState(this);
        Stats.isDead = false;
        FSM.ChangeState(IdleState);
    }
}
