using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FallingPiece : MonoBehaviour
{
    public struct MoveState : System.IFormattable
    {
        public float lastMoveTime;
        public bool isMovingContinuously;

        public MoveState(float lastMoveTime, bool isMovingContinuously)
        {
            this.lastMoveTime = lastMoveTime;
            this.isMovingContinuously = isMovingContinuously;
        }

        public static MoveState NotMoving { get { return new MoveState(Time.fixedTime, false); } }

        public string ToString(string Format, System.IFormatProvider Provider)
        {
            return $"(lastMoveTime: {lastMoveTime}, isMovingContinuously: {isMovingContinuously})";
        }

    }

    public enum RotationType
    {
        Clockwise,
        CounterClockwise
    }

    public const string GHOST_PIECE_NAME = "Ghost";

    // This is set dynamically via GameManager.
    [HideInInspector]
    public PieceData PieceData;

    private GameManager _gameManager;
    private MoveState _dropState;
    private MoveState _moveRightState;
    private MoveState _moveLeftState;
    // TODO: better floor-kick detection?
    private bool _hasFloorKicked;
    private GameObject _ghostPiece;

    // Start is called before the first frame update.
    void Start()
    {
        _gameManager = FindObjectOfType<GameManager>();
        // Use spawn time as initial "last dropped time" so that it doesn't drop immediately upon spawning.
        _dropState = new MoveState(Time.fixedTime, isMovingContinuously: false);
        _ghostPiece = Instantiate(this.gameObject, Vector3.zero, Quaternion.identity, this.transform);
        _ghostPiece.name = GHOST_PIECE_NAME;
        Destroy(_ghostPiece.GetComponent<FallingPiece>());
    }

    // Update is called once per frame
    void Update()
    {
        // Try moving left.
        if (!Input.GetKey(KeyCode.LeftArrow))
        {
            (MoveState newMoveRightState, bool shouldMove) = CheckMovementInput(Control.MoveRight, _moveRightState);
            _moveRightState = newMoveRightState;
            if (shouldMove) ApplyMovementIfValid(new Vector2(1, 0));
        }

        // Try moving right.
        if (!Input.GetKey(KeyCode.RightArrow))
        {
            (MoveState newMoveLeftState, bool shouldMove) = CheckMovementInput(Control.MoveLeft, _moveLeftState);
            _moveLeftState = newMoveLeftState;
            if (shouldMove) ApplyMovementIfValid(new Vector2(-1, 0));
        }

        // Try rotating clockwise.
        if (GameInput.GetControlDown(Control.RotateClockwise) && !GameInput.GetControlDown(Control.RotateCounterClockwise))
        {
            TryRotatingPiece(RotationType.Clockwise);
        }

        // Try rotating counter-clockwise.
        if (GameInput.GetControlDown(Control.RotateCounterClockwise) && !GameInput.GetControlDown(Control.RotateClockwise))
        {
            TryRotatingPiece(RotationType.CounterClockwise);
        }

        // Hard drop.
        bool hardDropped = false;
        if (GameInput.GetControlDown(Control.HardDrop))
        {
            Vector2 movement = GetHardDropMovement();
            ApplyMovementIfValid(movement);
            _gameManager.AddPieceToFallenTiles(this.gameObject);
        }

        // Soft drop.
        (MoveState newDropState, bool shouldDrop) = CheckMovementInput(Control.SoftDrop, _dropState);
        _dropState = newDropState;
        if (!hardDropped && (shouldDrop || (Time.fixedTime - _dropState.lastMoveTime > Constants.TIME_BETWEEN_DROPS)))
        {
            if (CheckMovementCollision(new Vector2(0, -1)))
            {
                _gameManager.AddPieceToFallenTiles(this.gameObject);
            }
            else
            {
                ApplyMovementIfValid(new Vector2(0, -1));
                _dropState.lastMoveTime = Time.fixedTime;
            }
        }

        UpdateGhostPiecePosition();
    }

    private (MoveState moveState, bool shouldMove) CheckMovementInput(Control moveControl, MoveState currentMoveState)
    {
        // If key isn't pressed down, obviously do nothing.
        if (!GameInput.GetControl(moveControl)) return (currentMoveState, shouldMove: false);

        // If this is the first frame the key is pressed, execute a single move.
        if (GameInput.GetControlDown(moveControl))
        {
            return (new MoveState(Time.fixedTime, isMovingContinuously: false), shouldMove: true);
        }
        // If key is held down, need to hold down for a certain threshold before the piece starts moving.
        else if (!currentMoveState.isMovingContinuously &&
                (Time.fixedTime - currentMoveState.lastMoveTime > Constants.TIME_BEFORE_CONTINUOUS_MOVE))
        {
            return (new MoveState(Time.fixedTime, isMovingContinuously: true), shouldMove: true);
        }
        // They've held the key down over initial threshold, now just dispatch moves every couple frames.
        else if (currentMoveState.isMovingContinuously &&
                (Time.fixedTime - currentMoveState.lastMoveTime > Constants.TIME_BETWEEN_MOVES))
        {
            return (new MoveState(Time.fixedTime, isMovingContinuously: true), shouldMove: true);
        }

        // Don't move, but keep old "lastMoveTime", since they are still holding down the key, 
        return (currentMoveState, shouldMove: false);
    }

    // Update ghost piece (or hide if it's in the same position as the main
    // piece).
    private void UpdateGhostPiecePosition()
    {
        for (int y = 1; y < Constants.BOARD_HEIGHT + 2; y++)
        {
            var movement = new Vector2(0, -y);
            if (CheckMovementCollision(movement))
            {
                if (y == 1)
                {
                    _ghostPiece.SetActive(false);
                }
                else
                {
                    // _ghostPiece.SetActive(true);
                    _ghostPiece.transform.position = this.transform.position + new Vector3(0, -y + 1, 0);
                }
                break;
            }
        }
    }

    private void TryRotatingPiece(RotationType rotationType)
    {
        // First, get wall kick offsets, depending on type of piece, current rotation, and desired rotation type.
        Vector2[] wallKickOffsets = GetWallKickOffsets(rotationType);

        Vector3 rotation = (rotationType) switch
        {
            RotationType.Clockwise => new Vector3(0, 0, -90),
            RotationType.CounterClockwise => new Vector3(0, 0, 90),
            _ => new Vector3(0, 0, 0)
        };
        foreach (var offset in wallKickOffsets)
        {
            // Prevent floor kick abuse by limiting to 1 floor kick per piece.
            bool isFloorKick = offset.y > 0;
            if (isFloorKick && _hasFloorKicked) continue;

            if (ApplyMovementAndRotationIfValid(offset, rotation))
            {
                if (isFloorKick) _hasFloorKicked = true;
                break;
            };
        }
    }

    // Wall kick offset values taken from https://tetris.wiki/Super_Rotation_System#Wall_Kicks.
    private Vector2[] GetWallKickOffsets(RotationType rotationType)
    {
        return (this.PieceData.WallKickType, this.transform.rotation.eulerAngles.y, rotationType) switch
        {
            /* J, L, S, T, Z PIECE WALL KICK OFFSETS */
            // 0->R
            (WallKickTypes.JLSTZ, 0, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -2), new Vector2(-1, -2) },
            // R->0
            (WallKickTypes.JLSTZ, -90, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, 2), new Vector2(1, 2) },
            // R->2
            (WallKickTypes.JLSTZ, -90, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, -1), new Vector2(0, 2), new Vector2(1, 2) },
            // 2->R
            (WallKickTypes.JLSTZ, 180, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(0, -2), new Vector2(-1, -2) },
            // 2->L
            (WallKickTypes.JLSTZ, 180, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, -2), new Vector2(1, -2) },
            // L->2
            (WallKickTypes.JLSTZ, 90, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, 2), new Vector2(-1, 2) },
            // L->0
            (WallKickTypes.JLSTZ, 90, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(-1, -1), new Vector2(0, 2), new Vector2(-1, 2) },
            // 0->L
            (WallKickTypes.JLSTZ, 0, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, -2), new Vector2(1, -2) },

            /* I PIECE WALL KICK OFFSETS */
            // 0->R
            (WallKickTypes.I, 0, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-2, 0), new Vector2(1, 0), new Vector2(-2, -1), new Vector2(1, 2) },
            // R->0
            (WallKickTypes.I, -90, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(2, 0), new Vector2(-1, 0), new Vector2(2, 1), new Vector2(-1, -2) },
            // R->2
            (WallKickTypes.I, -90, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(2, 0), new Vector2(-1, 2), new Vector2(2, -1) },
            // 2->R
            (WallKickTypes.I, 180, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(-2, 0), new Vector2(1, -2), new Vector2(-2, 1) },
            // 2->L
            (WallKickTypes.I, 180, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(2, 0), new Vector2(-1, 0), new Vector2(2, 1), new Vector2(-1, -2) },
            // L->2
            (WallKickTypes.I, 90, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-2, 0), new Vector2(1, 0), new Vector2(-2, -1), new Vector2(1, 2) },
            // L->0
            (WallKickTypes.I, 90, RotationType.Clockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(-2, 0), new Vector2(1, -2), new Vector2(-2, 1) },
            // 0->L
            (WallKickTypes.I, 0, RotationType.CounterClockwise) =>
                new Vector2[] { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(2, 0), new Vector2(-1, 2), new Vector2(2, -1) },

            /* O PIECE WALL KICK OFFSETS (no offsets lol) */
            (WallKickTypes.O, _, _) =>
               new Vector2[] { new Vector2(0, 0) },

            // Default case
            _ => new Vector2[] { new Vector2(0, 0) }
        };
    }

    // Returns a movement vector needed to get this piece to the lowest position it can go.
    private Vector2 GetHardDropMovement()
    {
        for (int y = 0; y < Constants.BOARD_HEIGHT + 5; y++)
        {
            bool collision = CheckMovementCollision(new Vector2(0, -y));
            if (collision) return new Vector2(0, -y + 1);
        }

        // This should never happen...
        return new Vector3(0, 0);
    }

    // Returns true if movement was successfully applied.
    private bool ApplyMovementIfValid(Vector2 movement)
    {
        return ApplyMovementAndRotationIfValid(movement, rotation: Vector3.zero);
    }

    // Returns true if rotation was successfully applied.
    private bool ApplyRotationIfValid(Vector3 rotation)
    {
        return ApplyMovementAndRotationIfValid(movement: Vector3.zero, rotation);
    }

    // Returns true if movement & rotation were successfully applied.
    private bool ApplyMovementAndRotationIfValid(Vector2 movement, Vector3 rotation)
    {
        if (!CheckCollision(movement, rotation))
        {
            transform.position += (Vector3)movement;
            transform.Rotate(rotation);
            return true;
        }

        return false;
    }

    // Returns true if new position results in a collision.
    private bool CheckMovementCollision(Vector2 movement)
    {
        return CheckCollision(movement, rotation: Vector3.zero);
    }

    // Returns true if new rotation results in a collision.
    private bool CheckRotationCollision(Vector3 rotation)
    {
        return CheckCollision(movement: Vector2.zero, rotation);
    }

    // Returns true if new position/rotation results in a collision.
    private bool CheckCollision(Vector2 movement, Vector3 rotation)
    {
        bool collisionFound = false;

        // Store old values, so we can revert if needed.
        Vector2 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;

        // Apply result temporarily, to check validity.
        transform.position += (Vector3)movement;
        transform.Rotate(rotation);

        // Check if any of the tiles on this piece are directly in the way of any of the fallen pieces.
        // This probably isn't super efficient, but there's not that many pieces...
        foreach (SpriteRenderer pieceTile in GetComponentsInChildren<SpriteRenderer>())
        {
            // Don't want to consider ghost pieces in collision. (Probably a
            // better way to do this ¯\_(ツ)_/¯).
            if (pieceTile.gameObject.transform.parent.name == GHOST_PIECE_NAME) continue;

            Vector2 tilePosition = pieceTile.transform.position;
            if (tilePosition.y < -Constants.BOARD_HEIGHT + 0.9) collisionFound = true;
            if (tilePosition.x < -0.01) collisionFound = true;
            if (tilePosition.x > Constants.BOARD_WIDTH - 0.9) collisionFound = true;

            foreach (GameObject fallenTile in _gameManager.FallenTiles)
            {
                if (fallenTile.transform.position == (Vector3)tilePosition)
                {
                    collisionFound = true;
                }
            }
        }

        // Undo transform, and return value.
        transform.position = oldPosition;
        transform.rotation = oldRotation;

        return collisionFound;
    }
}
