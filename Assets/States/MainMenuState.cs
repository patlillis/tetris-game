using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : StateChangeHandler
{

    public override void RegisterStateChangeHandlers(StateChangeRegistrar registrar)
    {
        // When entering main menu, set to active.
        registrar.RegisterStateEnteredHandler(State.MainMenu, (State previousState) =>
        {
            this.gameObject.SetActive(true);
        });

        // When exiting main menu, set to inactive.
        registrar.RegisterStateExitedHandler(State.MainMenu, (State nextState) =>
        {
            this.gameObject.SetActive(false);
        });
    }

    // When play button is clicked, transition states.
    public void OnPlayButtonClicked()
    {
        FindObjectOfType<GameManager>().GoToState(State.Gameplay);
    }

    // When exit button is clicked, quit everything.
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
