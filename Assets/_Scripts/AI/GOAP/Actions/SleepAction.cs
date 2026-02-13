using UnityEngine;
using UnityEngine.AI;

public class SleepAction : GoapAction
{
    private NPCStats stats;

    private float timer;
    private float sleepTime;

    public SleepAction(NPCStats stats)
    {
        Name = "Sleep";
        Cost = 1.5f;
        this.stats = stats;

        preconditions["AtBed"] = 1;
        preconditions["Alive"] = 1;

        effects["NotTired"] = 1;
    }

    public override void OnEnter()
    {
        timer = 0;
        sleepTime = GameTime.Day / 4;
        stats.isSleeping = true;
    }

    public override bool Perform(float deltaTime)
    {
        timer += deltaTime;
        if (timer >= sleepTime)
        {
            stats.AddFatigue(-80);
            return true;
        }
        return false;
    }

    public override void OnExit()
    {
        stats.isSleeping = false;
    }
}
