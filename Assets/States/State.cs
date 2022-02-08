
/*
State transition diagram:

┌───────┐
│Startup│
└───┬───┘
    │
┌───▼─────┐ ┌────────┐
│Main Menu├─►Gameplay│
└──────▲──┘ └┐▲──────┘
       │     ││
     ┌─┴─────▼└─┐
     │Pause Menu│
     └──────────┘

*/
public enum State
{
    Startup, // This isn't actually a real state, it's just the entry point of the game.
    MainMenu,
    Gameplay,
    PauseMenu,
}
