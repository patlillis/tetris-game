using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    // This isn't actually a real state, it's just the entry point of the game.
    Startup,
    MainMenu,
    Gameplay
}

public abstract class State : MonoBehaviour
{
    public abstract void OnStateEntered(States previousState);

    public abstract void OnStateExited(States nextState);
}
