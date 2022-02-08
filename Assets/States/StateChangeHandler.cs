using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    Startup, // This isn't actually a real state, it's just the entry point of the game.
    MainMenu,
    Gameplay,
    PauseMenu,
}

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
    private readonly Dictionary<States, List<Action<States>>> stateExitedHandlers = new Dictionary<States, List<Action<States>>>();
    private readonly Dictionary<(States, States), List<Action>> stateChangedHandlers = new Dictionary<(States, States), List<Action>>();
    private readonly Dictionary<States, List<Action<States>>> stateEnteredHandlers = new Dictionary<States, List<Action<States>>>();

    // Registers a handler for when a specific state is exited. The handler
    // delegate will be called, passing in the state being entered as a param.
    public void RegisterStateExitedHandler(States stateBeingExited, Action<States> handler)
    {
        if (!stateExitedHandlers.ContainsKey(stateBeingExited))
        {
            stateExitedHandlers[stateBeingExited] = new List<Action<States>>();
        }
        stateExitedHandlers[stateBeingExited].Add(handler);
    }

    // Registers a handler for when a specific state is transitioning to a
    // specific other state.
    public void RegisterStateChangeHandler(States stateBeingExited, States stateBeingEntered, Action handler)
    {
        if (!stateChangedHandlers.ContainsKey((stateBeingExited, stateBeingEntered)))
        {
            stateChangedHandlers[(stateBeingExited, stateBeingEntered)] = new List<Action>();
        }
        stateChangedHandlers[(stateBeingExited, stateBeingEntered)].Add(handler);
    }

    // Registers a handler for when a specific state is entered. The handler
    // delegate will be called, passing in the state being exited as a param.
    public void RegisterStateEnteredHandler(States stateBeingEntered, Action<States> handler)
    {
        if (!stateEnteredHandlers.ContainsKey(stateBeingEntered))
        {
            stateEnteredHandlers[stateBeingEntered] = new List<Action<States>>();
        }
        stateEnteredHandlers[stateBeingEntered].Add(handler);
    }

    // THIS SHOULD ONLY BE CALLED FROM GAME MANAGER!
    public void HandleStateChange(States stateBeingExited, States stateBeingEntered)
    {
        // Call state exited handlers
        if (stateExitedHandlers.ContainsKey(stateBeingExited))
        {
            foreach (Action<States> handler in stateExitedHandlers[stateBeingExited])
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
            foreach (Action<States> handler in stateEnteredHandlers[stateBeingEntered])
            {
                handler(stateBeingExited);
            }
        }
    }
}