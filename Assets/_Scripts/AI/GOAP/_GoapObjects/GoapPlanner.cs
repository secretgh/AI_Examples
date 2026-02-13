using System.Collections.Generic;
using System.Linq;

public class GoapPlanner
{
    private class Node
    {
        public Node Parent;
        public GoapAction Action;
        public WorldState State;
        public float Cost;
    }

    public Queue<GoapAction> Plan(
        WorldState start,
        IEnumerable<GoapAction> actions,
        GoapGoal goal)
    {
        var open = new List<Node>();
        var closed = new List<Node>();

        open.Add(new Node { State = start, Cost = 0 });

        while (open.Count > 0)
        {
            var current = open.OrderBy(n => n.Cost).First();
            open.Remove(current);

            if (GoalReached(current.State, goal))
                return BuildPlan(current);

            foreach (var action in actions)
            {
                if (!action.CanRun(current.State))
                    continue;

                var newState = ApplyEffects(current.State, action);
                open.Add(new Node
                {
                    Parent = current,
                    Action = action,
                    State = newState,
                    Cost = current.Cost + action.Cost
                });
            }

            closed.Add(current);
        }

        return null;
    }

    private bool GoalReached(WorldState state, GoapGoal goal)
    {
        foreach (var kv in goal.DesiredState)
        {
            //if state is less then desired it is not correct.
            if (state.Get(kv.Key) < kv.Value)
            {
                return false;
            }

        }
        return true;
    }

    private WorldState ApplyEffects(WorldState state, GoapAction action)
    {
        var newState = state.Clone();
        foreach (var e in action.Effects)
            newState.Set(e.Key, newState.Get(e.Key) + e.Value);
        return newState;
    }

    private Queue<GoapAction> BuildPlan(Node node)
    {
        var result = new Stack<GoapAction>();
        while (node.Action != null)
        {
            result.Push(node.Action);
            node = node.Parent;
        }
        return new Queue<GoapAction>(result);
    }
}
