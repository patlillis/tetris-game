using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    MainMenu,
    Gameplay
}

public abstract class State : MonoBehaviour
{
    public abstract void OnStateEntered(States previousState);

    public abstract void OnStateExited(States nextState);
}
