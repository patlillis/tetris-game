using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WallKickTypes
{
    O,
    I,
    JLSTZ
}

public class PieceData : MonoBehaviour
{
    public Vector2 SpawnPositionOffset;
    public Vector2 VisualCenterOffset;
    public WallKickTypes WallKickType;
}
