using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private States _currentState = States.Startup;

    private Dictionary<States, State> _stateObjects;

    public void Awake()
    {
        _stateObjects = new Dictionary<States, State>() {
            { States.MainMenu, FindObjectOfType<MainMenuState>() },
            { States.Gameplay, FindObjectOfType<GameplayState>() }
        };
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

        if (_stateObjects.TryGetValue(_currentState, out State currentStateObject) && currentStateObject != null)
        {
            currentStateObject.OnStateExited(newState);
        }
        if (_stateObjects.TryGetValue(newState, out State newStateObject) && newStateObject != null)
        {
            newStateObject.OnStateEntered(_currentState);
        }
        _currentState = newState;
    }
}
