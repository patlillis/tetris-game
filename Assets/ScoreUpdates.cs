using System;
using UnityEngine;

public static class ScoreUpdate
{
    public static int SoftDrop()
    {
        return 1;
    }

    public static int HardDrop(int tilesDropped)
    {
        return 2 * tilesDropped;
    }


    // TODO: "isBackToBackTetris" should be "isBackToBackDifficult" or something
    // See https://tetris.wiki/Scoring#Recent_guideline_compatible_games
    public static int LineClear(int linesCleared, bool isBackToBackTetris, int level)
    {
        if (linesCleared > 4)
        {
            Debug.Log($"Somehow cleared {linesCleared} lines!?!?!");
        }
        return linesCleared switch
        {
            1 => 100 * level,
            2 => 300 * level,
            3 => 500 * level,
            4 => (isBackToBackTetris ? 1200 : 800) * level,
            _ => 0
        };
    }

    public static int Combo(int comboCounter, int level)
    {
        return comboCounter * 50 * level;
    }

    public static int PerfectClear(int linesCleared, int level)
    {
        return linesCleared switch
        {
            1 => 800 * level,
            2 => 1200 * level,
            3 => 1800 * level,
            4 => 2000 * level,
            _ => 0
        };
    }

    public static int BackToBackPerfectTetris(int level)
    {
        return 3200 * level;
    }
}