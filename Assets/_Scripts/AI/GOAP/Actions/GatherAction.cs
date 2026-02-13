public class GatherAction : GoapAction
{
    private NPCStats stats;
    float timer = 0f;
    float duration = GameTime.Day / 8;
    float hungerImpact = 20f;
    float fatigueImpact = 20f;

    public GatherAction(NPCStats stats)
    {
        Name = "Gather";
        Cost = 2f;

        this.stats = stats;

        preconditions["AtBush"] = 1;
        preconditions["Alive"] = 1;

        effects["HasFood"] = 1;
    }


    public override bool Perform(float deltaTime)
    {
        timer += deltaTime;
        stats.AddFatigue((fatigueImpact/duration) * deltaTime);
        stats.AddHunger((hungerImpact/duration) * deltaTime);
        if (timer >= duration)
        {
            stats.food += 2;
            return true;
        }
        return false;
    }
}
