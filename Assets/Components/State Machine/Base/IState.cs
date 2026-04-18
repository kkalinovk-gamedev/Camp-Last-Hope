public interface IState
{
    void OnEnter();
    void Update();
    void PhysicsUpdate();
    void OnExit();
}
