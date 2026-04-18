using System;

public class FuncPredicate : IPredicate
{
    private readonly Func<bool> predicateFunction;

    public FuncPredicate(Func<bool> predicateFunction)
    {
        this.predicateFunction = predicateFunction;
    }

    public bool Evaluate() => predicateFunction.Invoke();
}
