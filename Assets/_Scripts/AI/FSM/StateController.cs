using System.Collections.Generic;

public class StateMachine
{
    public State CurrentState { get; private set; }
    public List<State> AllStates;

    public IEnumerable<State> GetPossibleNextStates()
    {
        foreach (var state in AllStates)
        {
            if(state != CurrentState && state.CanEnter())
            {
                yield return state;
            }
        }
    }

    public void ChangeState(State newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Update();
    }
}
