using System;

public class ConditionNode : Node
{
    Func<bool> condition;

    public ConditionNode(string name, Func<bool> condition) : base(name)
    {
        this.condition = condition;
    }

    public override NodeStatus Tick()
    {
        if (condition())
        {
            status = NodeStatus.Success;
            return NodeStatus.Success;
        } 
        else { 
            status = NodeStatus.Failure;
            return NodeStatus.Failure;
        } 
    }
}
