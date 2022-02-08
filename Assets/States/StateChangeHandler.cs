using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Components should inherit from this to handle state change "events".
//
// They're not real C# Events, because Events don't really play super well with
// generics. We want the handlers to be able to register for "MainMenu State
// Exited", not just the overall "State Exited".
public abstract class StateChangeHandler : MonoBehaviour
{
    public abstract void RegisterStateChangeHandlers(StateChangeRegistrar registrar);
}

// Handles keeping track of which handlers to call for which events.
//
// Components should only need to worry about this class when implementing the
// "RegisterStateChangeHandlers()" abstract method.
public sealed class StateChangeRegistrar
{
    private readonly Dictionary<State, List<Action<State>>> stateExitedHandlers = new Dictionary<State, List<Action<State>>>();
    private readonly Dictionary<(State, State), List<Action>> stateChangedHandlers = new Dictionary<(State, State), List<Action>>();
    private readonly Dictionary<State, List<Action<State>>> stateEnteredHandlers = new Dictionary<State, List<Action<State>>>();

    // Registers a handler for when a specific state is exited. The handler
    // delegate will be called, passing in the state being entered as a param.
    public void RegisterStateExitedHandler(State stateBeingExited, Action<State> handler)
    {
        if (!stateExitedHandlers.ContainsKey(stateBeingExited))
        {
            stateExitedHandlers[stateBeingExited] = new List<Action<State>>();
        }
        stateExitedHandlers[stateBeingExited].Add(handler);
    }

    // Registers a handler for when a specific state is transitioning to a
    // specific other state.
    public void RegisterStateChangeHandler(State stateBeingExited, State stateBeingEntered, Action handler)
    {
        if (!stateChangedHandlers.ContainsKey((stateBeingExited, stateBeingEntered)))
        {
            stateChangedHandlers[(stateBeingExited, stateBeingEntered)] = new List<Action>();
        }
        stateChangedHandlers[(stateBeingExited, stateBeingEntered)].Add(handler);
    }

    // Registers a handler for when a specific state is entered. The handler
    // delegate will be called, passing in the state being exited as a param.
    public void RegisterStateEnteredHandler(State stateBeingEntered, Action<State> handler)
    {
        if (!stateEnteredHandlers.ContainsKey(stateBeingEntered))
        {
            stateEnteredHandlers[stateBeingEntered] = new List<Action<State>>();
        }
        stateEnteredHandlers[stateBeingEntered].Add(handler);
    }

    // THIS SHOULD ONLY BE CALLED FROM GAME MANAGER!
    public void HandleStateChange(State stateBeingExited, State stateBeingEntered)
    {
        // Call state exited handlers
        if (stateExitedHandlers.ContainsKey(stateBeingExited))
        {
            foreach (Action<State> handler in stateExitedHandlers[stateBeingExited])
            {
                handler(stateBeingEntered);
            }
        }

        // Then call state changed handlers
        if (stateChangedHandlers.ContainsKey((stateBeingExited, stateBeingEntered)))
        {
            foreach (Action handler in stateChangedHandlers[(stateBeingExited, stateBeingEntered)])
            {
                handler();
            }
        }

        // Then call state entered handlers.
        if (stateEnteredHandlers.ContainsKey(stateBeingEntered))
        {
            foreach (Action<State> handler in stateEnteredHandlers[stateBeingEntered])
            {
                handler(stateBeingExited);
            }
        }
    }
}