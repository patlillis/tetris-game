using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : State
{
    public override void OnStateEntered(States previousState)
    {
        this.gameObject.SetActive(true);
    }

    public override void OnStateExited(States nextState)
    {
        this.gameObject.SetActive(false);
    }

    public void OnPlayButtonClicked()
    {
        FindObjectOfType<GameManager>().GoToState(States.Gameplay);
    }

    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game.
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
