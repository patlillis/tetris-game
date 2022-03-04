using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    // Taken from Tetris Guideline https://tetris.wiki/Marathon.
    public static float DROP_TIME_FOR_LEVEL(int level)
    {
        // Gravity calculation is really only used for levels 1-20. If less than
        // 1, use level 1's gravity. If greather than 20, use level 20's gravity.
        int clampedLevel = Mathf.Clamp(level, 1, 20);

        return Mathf.Pow(0.8f - ((clampedLevel - 1) * 0.007f), clampedLevel - 1);
    }

    // DAS values taken from Tetris Guideline https://tetris.wiki/Tetris_Guideline.
    public static readonly float AUTO_SHIFT_DELAY_SECONDS = 0.167f;
    public static readonly float AUTO_REPEAT_RATE_SECONDS = 0.033f;
    public static readonly int BOARD_HEIGHT = 20;
    public static readonly int BOARD_WIDTH = 10;

    // Changing this would also require scene & UI update, but better to have the
    // number in one place so ¯\_(ツ)_/¯
    public static readonly int NEXT_PIECES_COUNT = 3;

    public enum ScoreUpdateEvent
    {
        SoftDrop
    }
}