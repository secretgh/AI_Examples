using System.Collections.Generic;
using System.Linq;

public class SelectorNode : Node
{
    public int currentChildIndex { get; private set; } = -1;

    public SelectorNode(string name, params Node[] list) : base(name) { 
        children = list.ToList();
    }

    public override NodeStatus Tick()
    {
        for (int i = 0; i < children.Count; i++) { 
            currentChildIndex = i;
            var child = children[i];
            var result = child.Tick();
            if (result == NodeStatus.Running) { 
                status = result;
               return result;
            }
            if (result == NodeStatus.Success)
            { 
                status = result;
                return result;
            }
        }

        currentChildIndex = -1;
        status = NodeStatus.Failure;
        return NodeStatus.Failure;
    }
}
