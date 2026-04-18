public interface ITransition
{
    IState To { get; }
    IPredicate Predicate { get; }
    bool CanTransition();
    void OnTransition();
}
