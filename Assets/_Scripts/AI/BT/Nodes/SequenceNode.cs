using System.Collections.Generic;
using System.Linq;

public class SequenceNode : Node
{
    int currentIndex = 0;

    public SequenceNode(string name, params Node[] nodes) : base(name) {
        children = nodes.ToList();
    }

    public override NodeStatus Tick()
    {
        while (currentIndex < children.Count)
        {
            var status = children[currentIndex].Tick();

            if (status == NodeStatus.Running) { 
                status = NodeStatus.Running;
                return NodeStatus.Running;
            }

            if (status == NodeStatus.Failure)
            {
                currentIndex = 0;
                status = NodeStatus.Failure;
                return NodeStatus.Failure;
            }

            currentIndex++;
        }

        currentIndex = 0;
        status = NodeStatus.Success;
        return NodeStatus.Success;
    }
}
