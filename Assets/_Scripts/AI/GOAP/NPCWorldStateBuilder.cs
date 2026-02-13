using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class NPCWorldStateBuilder
{
    public static WorldState FromStats(AIController controller)
    {
        var state = new WorldState();
        NPCStats stats = controller.Stats;

        // Core stats
        state.Set("Alive", stats.health > 0 ? 1 : 0);

        state.Set("NotHungry", stats.hunger <= 40 ? 1 : 0);
        state.Set("NotTired", stats.fatigue <= 70 ? 1 : 0);
        state.Set("HasFood", stats.food);


        // Location-based states for movement
        state.Set("AtBed", controller.agent.destination == controller.Bed.position && controller.agent.isStopped ? 1 : 0);
        state.Set("AtBush", controller.agent.destination == controller.Bed.position && controller.agent.isStopped ? 1 : 0);

        return state;
    }
}
