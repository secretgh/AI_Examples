public abstract class BehaviorTree
{
    public Node root { get; protected set; }

    public abstract Node BuildTree();

    public void Tick()
    {
        root.Tick();
    }
}