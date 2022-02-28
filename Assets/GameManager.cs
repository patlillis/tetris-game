using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private State _currentState = State.Startup;

    private readonly StateChangeRegistrar _stateChangeRegistrar = new StateChangeRegistrar();

    public void Awake()
    {
        foreach (StateChangeHandler stateChangeHandler in FindObjectsOfType<StateChangeHandler>(true))
        {
            stateChangeHandler.RegisterStateChangeHandlers(_stateChangeRegistrar);
        }
    }

    public void Start()
    {
        GoToState(State.MainMenu);
    }

    public void GoToState(State newState)
    {
        if (newState == State.Startup)
        {
            throw new ArgumentException("Can't go to Startup state.");
        }


        _stateChangeRegistrar.HandleStateChange(_currentState, newState);
        _currentState = newState;
    }
}
