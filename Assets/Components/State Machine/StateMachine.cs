using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;

public class StateMachine : SerializedMonoBehaviour
{
    [ShowInInspector, LabelText("Current State")]
    private string CurrentStateName => currentNode?.State.GetType().Name ?? "None";

    public Type CurrentState => currentNode?.State.GetType();

    private StateNode currentNode;
    private Dictionary<Type, StateNode> stateNodes = new();
    private HashSet<ITransition> globalTransitions = new();
    private bool isActive = false;

    public void Initialize<TState>() where TState : IState
    {
        var initialStateType = typeof(TState);

        if (stateNodes.TryGetValue(initialStateType, out var node))
        {
            currentNode = node;
        }
        else
        {
            throw new Exception($"State {initialStateType.Name} not found in StateMachine. Please add the state before setting it as initial.");
        }
    }

    public void Activate()
    {
        isActive = true;
        currentNode?.State.OnEnter();
    }

    public void Deactivate()
    {
        isActive = false;
        currentNode?.State.OnExit();
    }

    public void AddState(IState state)
    {
        var stateType = state.GetType();
        if (!stateNodes.ContainsKey(stateType))
        {
            stateNodes[stateType] = new StateNode(state);
        }
    }

    public void AddGlobalTransition(IState to, IPredicate predicate)
    {
        if (stateNodes.ContainsKey(to.GetType()) == false)
        {
            stateNodes[to.GetType()] = new StateNode(to);
        }

        globalTransitions.Add(new Transition(to, predicate));
    }

    public void AddTransition(IState from, IState to, IPredicate predicate)
    {
        var fromState = from.GetType();
        var toState = to.GetType();

        if (stateNodes.ContainsKey(fromState) == false)
        {
            stateNodes[fromState] = new StateNode(to);
        }

        if (stateNodes.ContainsKey(toState) == false)
        {
            stateNodes[toState] = new StateNode(to);
        }

        stateNodes[fromState].Transitions.Add(new Transition(to, predicate));
    }

    private void Update()
    {
        if (!isActive)
            return;

        var transition = GetTransition();

        if (transition != null)
        {
            var toStateType = transition.To.GetType();

            if (stateNodes.TryGetValue(toStateType, out var nextNode))
            {
                currentNode?.State.OnExit();
                transition.OnTransition();
                currentNode = nextNode;
                currentNode.State.OnEnter();
            }
        }

        currentNode?.State.Update();
    }

    private void FixedUpdate()
    {
        if (!isActive)
            return;

        currentNode?.State.PhysicsUpdate();
    }

    private ITransition GetTransition()
    {
        foreach (var transition in globalTransitions)
        {
            if (transition.CanTransition())
                return transition;
        }

        foreach (var transition in currentNode?.Transitions)
        {
            if (transition.CanTransition())
                return transition;
        }

        return null;
    }
}
