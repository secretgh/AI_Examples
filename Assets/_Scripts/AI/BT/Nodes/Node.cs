using System.Collections.Generic;

public abstract class Node
{
    public enum NodeStatus
    {
        Success, Failure, Running, Inactive
    }
    public string name;
    public NodeStatus status { get; protected set; } = NodeStatus.Inactive;
    public Node parent { get; set; }
    public List<Node> children { get; protected set; }

    public Node(string name)
    {
        this.name = name;   
    }

    public abstract NodeStatus Tick();
}