using System;
using System.Collections.Generic;

// As an abstract class, only the base functionality is here.
// The rest is left up to the particular implementation.

public class TurnBasedFsm
{
    readonly Dictionary<Type, BaseState> register = new Dictionary<Type, BaseState>();

    readonly Stack<BaseState> stack = new Stack<BaseState>();

    /// <summary>  Boolean that indicates whether the FSM has been initialized or not. </summary>
    public bool IsInitialized { get; protected set; }    
    
    /// <summary>  Returns the state on the top of the stack. Can be null. </summary>
    public BaseState Current => PeekState();

    /// <summary>  Peeks a state from the stack. A peek returns null if the stack is empty. It doesn't trigger any call. </summary>
    public BaseState PeekState() => stack.Count > 0 ? stack.Peek() : null;


    /// <summary>  Register a state into the state machine. </summary>
    public void RegisterState(BaseState state)
    {
        if (state == null)
            throw new ArgumentNullException("Null is not a valid state.");
        
        var type = state.GetType();
        register.Add(type, state);
    }


    /// <summary>  Initialize all states. All states must be registered before initialization. </summary>
    public void Initialize()
    {
        // Clear to ensure a clean start.
        Clear();
        
        OnBeforeInitialize();

        foreach (var state in register.Values)
            state.OnInitialize();

        IsInitialized = true;
        OnAfterInitialize();
    }


    /// <summary>  Do something before the initialization. </summary>
    private void OnBeforeInitialize()
    {
        // Create states.
        var startState = new StartState(this);
        var idleState = new IdleState(this);
        // more
        // more
        
        // Register all states.
        RegisterState(startState);
        RegisterState(idleState);
        // more
        // more
    }

    /// <summary>  Do something after the initialization. </summary>
    private void OnAfterInitialize()
    {
        if (!IsInitialized)
            return;

        PopState();
        AdvanceState<StartState>();
    }
    
    /// <summary>  Update the state on the top of the stack. </summary>
    public void Update() => Current?.OnUpdate();

    /// <summary>  Checks if a state is the current state. </summary>
    // public bool IsCurrent<T>() where T : IState => Current?.GetType() == typeof(T);

    /// <summary>  Checks if a state is the current state. </summary>
    public bool IsCurrent(BaseState state)
    {
        if (state == null)
            throw new ArgumentNullException();

        return Current?.GetType() == state.GetType();
    }


    /// <summary>  Takes a state type, creates a state object, and pushes it onto the stack. </summary>
    public void AdvanceState<T>() where T: BaseState
    {
        var stateType = typeof(T);

        if (!register.ContainsKey(stateType))
            throw new ArgumentException("State " + stateType + " not registered yet.");
        
        var state = register[stateType];

        PopState();
        stack.Push(state);
        state.OnEnterState();
    }

    /// <summary>  Advance to the next game state. </summary>
    public void AdvanceState(BaseState state)
    {
        var type = state.GetType();
        if (!register.ContainsKey(type))
            throw new ArgumentException("State " + state + " not registered yet.");

        PopState();
        stack.Push(state);
        state.OnEnterState();
    }


    /// <summary>  Pops a state from the stack. </summary>
    public void PopState()
    {
        if (Current == null)
            return;

        var state = stack.Pop();
        state.OnExitState();

    }

    /// <summary>  Clears and restart the states register. </summary>
    public void Clear()
    {
        foreach (var state in register.Values)
            state.OnClear();

        stack.Clear();
        register.Clear();
    }



}