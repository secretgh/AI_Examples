using UnityEngine;
using UnityEngine.AI;

public class NPC_Normal : BehaviorTree
{
    NPCStats stats;
    NavMeshAgent agent;
    Transform Bed;
    Transform Bush;

    public NPC_Normal(AIController controller)
    {
        this.stats = controller.Stats;
        this.agent = controller.agent;
        this.Bed = controller.Bed;
        this.Bush = controller.Bush;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        Node tree = 
            new SelectorNode("Survival Selector",
                new SequenceNode("Dead Sequence",
                    new ConditionNode("No Health?", () => stats.health <= 0),
                    new ActionNode("Die", 0, 
                    onStart:() => { stats.isDead = true; }, 
                    null)
                ),
                new SequenceNode("Passed Out Sequence",
                    new ConditionNode("Fatigue 100?", () => stats.fatigue >= 100),
                    new ActionNode("Collaspe for a 1/2 Day",
                        GameTime.Day * 0.5f,
                        onStart:() => {
                            stats.isSleeping = true;
                        },
                        onComplete:() => {
                            stats.AddFatigue(-100);
                            stats.isSleeping = false;
                        })
                ),
                new SequenceNode("Eat Sequence",
                    new ConditionNode("Hunger > 60 and Has Food?", () => !stats.isSleeping && stats.hunger >= 60 && stats.food > 0),
                    new ActionNode("Consume Food, reduce hunger.",
                        GameTime.Day * 0.05f,
                        onStart:() => {
                            stats.isEating = true;
                        },
                        onComplete:() => {
                            stats.AddHunger(-40);
                            stats.AddFood(-1);
                            stats.isEating = false;
                        })
                ),
                new SequenceNode("Sleep Sequence",
                    new ConditionNode("Fatigue > 70?",() => stats.fatigue <= 100 && stats.fatigue >= 70),
                    new MoveToNode("Walk to Bed", agent, Bed),
                    new ActionNode("Sleep for 1/4 Day",
                        GameTime.Day * 0.25f,
                        onStart:() => {
                            stats.isSleeping = true;
                        },
                        onComplete: () => {
                            stats.AddFatigue(-80);
                            stats.isSleeping = false;
                        })
                ),
                new SequenceNode("Gather Sequence",
                    new ConditionNode("No Food?",() => stats.food <= 0),
                    new MoveToNode("Walk to Bush", agent, Bush),
                    new ActionNode("Gather food, increase hunger and fatigue.",
                        GameTime.Day * 0.08f,
                        onStart:() => {},
                        onComplete: () => {
                            stats.AddFood(3);
                            stats.AddHunger(15);
                            stats.AddFatigue(20);
                        })
                )
            );

        return tree;
    }
}