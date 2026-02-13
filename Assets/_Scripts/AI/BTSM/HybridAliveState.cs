using UnityEngine;

/// <summary>
/// Alive State - Normal operational state.
/// Uses a Behavior Tree to make complex decisions about survival needs.
/// The BT evaluates priorities: eating, sleeping, gathering food, etc.
/// </summary>
public class HybridAliveState : HybridState
{
    private Hybrid_AIController controller;

    public HybridAliveState(Hybrid_AIController controller)
    {
        this.controller = controller;
        Name = "Alive";
        
        // Build the behavior tree for this state
        behaviorTree = new AliveBehaviorTree(controller);
    }

    public override int Priority => 0; // Normal priority

    public override void Enter()
    {
        base.Enter();
        controller.agent.isStopped = false;
    }

    public override void Update()
    {
        // The base class Update() will tick the behavior tree
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
        // Clean up any ongoing actions
        controller.Stats.isEating = false;
        controller.Stats.isSleeping = false;
        controller.agent.ResetPath();
    }

    public override bool CanEnter()
    {
        // Can be in Alive state if not dead and not passed out
        return controller.Stats.health > 0 && controller.Stats.fatigue < 100;
    }
}

/// <summary>
/// Behavior Tree for the Alive state.
/// Handles all survival decision-making: eating, sleeping, gathering, idle behavior.
/// Uses a selector pattern to prioritize actions based on urgency.
/// </summary>
public class AliveBehaviorTree : BehaviorTree
{
    private Hybrid_AIController controller;
    private NPCStats stats;

    public AliveBehaviorTree(Hybrid_AIController controller)
    {
        this.controller = controller;
        this.stats = controller.Stats;
        root = BuildTree();
    }

    public override Node BuildTree()
    {
        // Main selector: Choose the most urgent action
        Node tree = new SelectorNode("Survival Priority Selector",
            
            // Priority 1: Critical Hunger - Immediate eating if starving
            new SequenceNode("Critical Hunger Response",
                new ConditionNode("Starving and Has Food?", 
                    () => stats.hunger >= 80 && stats.food > 0 && !stats.isSleeping && !stats.isEating),
                new ActionNode("Emergency Eat",
                    GameTime.Day * 0.03f, // Quick eating
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Emergency eating - critical hunger!");
                        stats.isEating = true;
                        controller.agent.ResetPath();
                    },
                    onComplete: () => {
                        stats.AddHunger(-50);
                        stats.AddFood(-1);
                        stats.isEating = false;
                    })
            ),

            // Priority 2: High Fatigue - Need to sleep soon
            new SequenceNode("High Fatigue Sleep",
                new ConditionNode("Very Tired?", 
                    () => stats.fatigue >= 80 && stats.fatigue < 100 && !stats.isSleeping && !stats.isEating),
                new MoveToNode("Go to Bed Urgently", controller.agent, controller.Bed),
                new ActionNode("Sleep (High Fatigue)",
                    GameTime.Day * 0.3f,
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Sleeping - high fatigue");
                        stats.isSleeping = true;
                    },
                    onComplete: () => {
                        stats.AddFatigue(-90);
                        stats.isSleeping = false;
                    })
            ),

            // Priority 3: Regular Eating
            new SequenceNode("Regular Eating",
                new ConditionNode("Hungry and Has Food?", 
                    () => stats.hunger >= 60 && stats.food > 0 && !stats.isSleeping && !stats.isEating),
                new ActionNode("Eat Meal",
                    GameTime.Day * 0.05f,
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Eating a meal");
                        stats.isEating = true;
                        controller.agent.ResetPath();
                    },
                    onComplete: () => {
                        stats.AddHunger(-40);
                        stats.AddFood(-1);
                        stats.isEating = false;
                    })
            ),

            // Priority 4: Normal Sleep
            new SequenceNode("Normal Sleep",
                new ConditionNode("Tired?", 
                    () => stats.fatigue >= 70 && stats.fatigue < 80 && !stats.isSleeping && !stats.isEating),
                new MoveToNode("Go to Bed", controller.agent, controller.Bed),
                new ActionNode("Sleep (Normal)",
                    GameTime.Day * 0.25f,
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Going to sleep");
                        stats.isSleeping = true;
                    },
                    onComplete: () => {
                        stats.AddFatigue(-80);
                        stats.isSleeping = false;
                    })
            ),

            // Priority 5: Gather Food - No food available
            new SequenceNode("Gather Food",
                new ConditionNode("Out of Food?", 
                    () => stats.food <= 0 && !stats.isSleeping && !stats.isEating),
                new MoveToNode("Go to Berry Bush", controller.agent, controller.Bush),
                new ActionNode("Gather Berries",
                    GameTime.Day * 0.1f,
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Gathering food");
                    },
                    onComplete: () => {
                        stats.AddFood(Random.Range(2, 4));
                        stats.AddHunger(10); // Gathering makes you a bit hungry
                        stats.AddFatigue(15); // Gathering is tiring
                    })
            ),

            // Priority 6: Proactive Food Gathering - Low food stock
            new SequenceNode("Stock Up Food",
                new ConditionNode("Low Food Stock?", 
                    () => stats.food <= 1 && stats.hunger < 50 && stats.fatigue < 60 && !stats.isSleeping && !stats.isEating),
                new MoveToNode("Go to Berry Bush", controller.agent, controller.Bush),
                new ActionNode("Gather More Berries",
                    GameTime.Day * 0.1f,
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Stocking up on food");
                    },
                    onComplete: () => {
                        stats.AddFood(Random.Range(2, 4));
                        stats.AddHunger(10);
                        stats.AddFatigue(15);
                    })
            ),

            // Priority 7: Rest/Idle - Nothing urgent to do
            new SequenceNode("Idle/Rest",
                new ConditionNode("Nothing Urgent?", () => true), // Always succeeds
                new ActionNode("Idle",
                    GameTime.Day * 0.05f, // Idle for a bit
                    onStart: () => {
                        Debug.Log("[Hybrid BT] Idling - all needs met");
                        controller.agent.ResetPath();
                    },
                    onComplete: () => {
                        // Small health regeneration while idle if well-fed
                        if (stats.hunger < 30 && stats.fatigue < 30)
                        {
                            stats.AddHealth(2);
                        }
                    })
            )
        );

        return tree;
    }
}
