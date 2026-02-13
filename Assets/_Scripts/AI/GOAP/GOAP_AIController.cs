using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class GoapNPCController : AIController
{
    private GoapPlanner planner = new();
    public Queue<GoapAction> currentPlan;
    public GoapGoal currentGoal;
    public GoapAction currentAction = null;

    private List<GoapAction> actions = new();
    private List<GoapGoal> goals = new();

    void Awake()
    {
        actions.Add(new MoveAction(agent, Bush, "AtBush"));
        actions.Add(new MoveAction(agent, Bed, "AtBed"));
        actions.Add(new EatAction(Stats));
        actions.Add(new GatherAction(Stats));
        actions.Add(new SleepAction(Stats));

        goals.Add(
            new GoapGoal
            {
                Name = "Eat",
                Priority = 1+(int)Stats.hunger,
                DesiredState = { ["Alive"] = 1, ["NotHungry"] = 1 }
            });
        goals.Add(
            new GoapGoal
            {
                Name = "Sleep",
                Priority = 1 + (int)Stats.fatigue,
                DesiredState = { ["Alive"] = 1, ["NotTired"] = 1}
            });
        goals.Add(
            new GoapGoal
            {
                Name = "Gather",
                Priority = (int)Stats.hunger - Stats.food*10,
                DesiredState = { ["Alive"] = 1, ["HasFood"] = 0 }
            });

    }

    void Update()
    {
        //dead
        if (Stats.health <= 0)
            return;

        //start planning
        if (currentAction == null && (currentPlan == null || currentPlan.Count == 0))
            BuildPlan();

        //Find an action in plan
        if (currentAction == null && currentPlan?.Count > 0)
        {
            currentAction = currentPlan.Dequeue();
            currentAction.OnEnter();
        }

        //execute the plan using an action
        if (currentAction != null)
        {
            if (currentAction.Perform(Time.deltaTime))
            {
                currentAction.OnExit();
                currentAction = null;
            }
        }
    }

    private void BuildPlan()
    {
        var world = NPCWorldStateBuilder.FromStats(this);

        foreach (var goal in goals.OrderByDescending(g=>g.Priority)) { 
            if(!goal.CanRun(world)) 
                continue;

            var p = planner.Plan(world, actions, goal);
            if(p != null)
            {
                currentGoal = goal;
                currentPlan = p;
                break;
            }
        }

    }
}
