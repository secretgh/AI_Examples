using UnityEngine;

public class EatAction : GoapAction
{
    private NPCStats stats;
    private float duration = GameTime.Day * 0.05f;
    private float timer;

    public EatAction(NPCStats stats)
    {
        Name = "Eat";
        Cost = 1f;
        this.stats = stats;

        //preconditions: must be met before can run
        preconditions["HasFood"] = 1;
        preconditions["Alive"] = 1;

        effects["NotHungry"] = 1;
    }

    public override void OnEnter()
    {
        timer = 0;
        stats.isEating = true;
    }

    public override bool Perform(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= duration)
        {
            stats.food--;
            stats.AddHunger(-40);
            return true;
        }
        return false;
    }

    public override void OnExit()
    {
        stats.isEating = false;
    }
}
