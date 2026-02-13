using System.Collections.Generic;

public abstract class GoapAction
{
    public string Name { get; protected set; }
    public float Cost { get; protected set; } = 1f;

    protected Dictionary<string, int> preconditions = new();
    protected Dictionary<string, int> effects = new();

    public IReadOnlyDictionary<string, int> Preconditions => preconditions;
    public IReadOnlyDictionary<string, int> Effects => effects;

    public virtual bool CanRun(WorldState state) {
        foreach (var precondition in preconditions)
        {
            if (state.Get(precondition.Key) < precondition.Value)
            {
                return false;
            }
        }
        return true;
    }

    // Called once when the action starts
    public virtual void OnEnter() { }

    // Called every frame while running
    public abstract bool Perform(float deltaTime);

    // Called when finished
    public virtual void OnExit() { }
}