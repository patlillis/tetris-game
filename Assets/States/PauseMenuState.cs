using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuState : StateChangeHandler
{
    public override void RegisterStateChangeHandlers(StateChangeRegistrar registrar)
    {
        // On game startup, hide pause menu.
        registrar.RegisterStateExitedHandler(State.Startup, (newState) =>
        {
            this.gameObject.SetActive(false);
        });

        // When entering pause menu, set to active.
        registrar.RegisterStateEnteredHandler(State.PauseMenu, (State previousState) =>
        {
            this.gameObject.SetActive(true);
        });

        // When exiting pause menu, set to inactive.
        registrar.RegisterStateExitedHandler(State.PauseMenu, (State nextState) =>
        {
            this.gameObject.SetActive(false);
        });
    }

    // When resume button is clicked, transition states.
    public void OnResumeButtonClicked()
    {
        FindObjectOfType<GameManager>().GoToState(State.Gameplay);
    }

    // When main menu button is clicked, transition states.
    public void OnMainMenuButtonClicked()
    {
        FindObjectOfType<GameManager>().GoToState(State.MainMenu);
    }
}
