using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private States _currentState = States.Startup;

    private readonly StateChangeRegistrar _stateChangeRegistrar = new StateChangeRegistrar();

    public void Awake()
    {
        foreach (StateChangeHandler stateChangeHandler in FindObjectsOfType<StateChangeHandler>())
        {
            stateChangeHandler.RegisterStateChangeHandlers(_stateChangeRegistrar);
        }
    }

    public void Start()
    {
        GoToState(States.MainMenu);
    }

    public void GoToState(States newState)
    {
        if (newState == States.Startup)
        {
            throw new ArgumentException("Can't go to Startup state.");
        }

        _stateChangeRegistrar.HandleStateChange(_currentState, newState);
        _currentState = newState;
    }
}
