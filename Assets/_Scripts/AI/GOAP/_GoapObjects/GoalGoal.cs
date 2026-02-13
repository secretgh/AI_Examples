using System.Collections.Generic;

public class GoapGoal
{
    public string Name;
    public Dictionary<string, int> DesiredState = new();
    public int Priority;

    public bool CanRun(WorldState world)
    {
        foreach (string state in DesiredState.Keys)
        {
            //If not in the required state then we should run to get into the desired state.
            if (world.Get(state) != DesiredState[state]) {
                return true;
            }
        }
        return false;
    }
}
