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

    public static int LineClear(int linesCleared, int comboCounter, int level)
    {
        if (linesCleared > 4)
        {
            Debug.Log($"Somehow cleared {linesCleared} lines!?!?!");
        }
        int lineClearScore = linesCleared switch
        {
            1 => 100 * level,
            2 => 300 * level,
            3 => 500 * level,
            4 => 800 * level,
            _ => 0
        };

        int comboScore = Mathf.Max(comboCounter * 50 * level, 0);

        return lineClearScore + comboScore;
    }
}