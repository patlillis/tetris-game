using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenuState : StateChangeHandler
{
    public Text ScoreText;

    public override void RegisterStateChangeHandlers(StateChangeRegistrar registrar)
    {
        // On game startup, hide game over menu.
        registrar.RegisterStateExitedHandler(State.Startup, (newState) =>
        {
            this.gameObject.SetActive(false);
        });

        // When entering game over menu, set to active and update score.
        registrar.RegisterStateEnteredHandler(State.GameOverMenu, (State previousState) =>
        {
            // There is probably a better way to do this, but score is never
            // going to change once they hit this menu.
            var GameplayState = FindObjectOfType<GameplayState>();
            int score = GameplayState.Score;
            ScoreText.text = score.ToString();

            this.gameObject.SetActive(true);
        });

        // When exiting game over menu, set to inactive.
        registrar.RegisterStateExitedHandler(State.GameOverMenu, (State nextState) =>
        {
            this.gameObject.SetActive(false);
        });
    }

    // When main menu button is clicked, transition states.
    public void OnMainMenuButtonClicked()
    {
        FindObjectOfType<GameManager>().GoToState(State.MainMenu);
    }
}
