using System.Collections.Generic;

public class WorldState
{
    private Dictionary<string, int> worldValues = new();

    public int Get(string key)
    {
        return worldValues.TryGetValue(key, out int value) ? value : 0;
    }

    public void Set(string key, int value)
    {
        worldValues[key] = value;
    }


    public bool MeetsConditions(Dictionary<string, int> conditions)
    {
        foreach (var condition in conditions)
        {
            if(Get(condition.Key) < condition.Value){
                return false;
            }
        }
        return true;
    }

    public WorldState Clone()
    {
        var clone = new WorldState();
        foreach (var worldValue in worldValues) {
            clone.worldValues[worldValue.Key] = worldValue.Value;
        }
        return clone;
    }
}
