public interface State
{
    string Name { get; }
    void Enter();
    void Update();
    void Exit();
    bool CanEnter();
}
