
/*
State transition diagram:

               ┌──────────────┐
               │Game Over Menu│
               └─┬─────▲──────┘
                 │     │
┌───────┐ ┌──────▼──┐ ┌┴───────┐
│Startup├─►Main Menu├─►Gameplay│
└───────┘ └──────▲──┘ └┐▲──────┘
                 │     ││
               ┌─┴─────▼└─┐
               │Pause Menu│
               └──────────┘

https://asciiflow.com/#/share/eJyrVspLzE1VssorzcnRUcpJrEwtUrJSqo5RqohRsrI0sdSJUaoEsowsLICsktSKEiAnRkkBFTyasockFBOTh2mCO9AdCv5lqUUKvql5pbhUYUfTNhFpD9gMGA2UxuNIHLZNI6gCyXYgGVySWFRSWgDTvcs3MTMP5kOICMjjBcCQJ9tFmzBUkBceJIY5gfgMSCwtTiUjLpHdq1SrVAsAEtNkdg%3D%3D)

*/
public enum State
{
    Startup, // This isn't actually a real state, it's just the entry point of the game.
    MainMenu,
    Gameplay,
    PauseMenu,
    GameOverMenu
}
