using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameplayState : StateChangeHandler
{
    public override void RegisterStateChangeHandlers(StateChangeRegistrar registrar)
    {
        // When going from main menu to gameplay, set to active and spawn new piece.
        registrar.RegisterStateChangeHandler(State.MainMenu, State.Gameplay, () =>
       {
           // TODO: countdown
           this.gameObject.SetActive(true);
           SpawnNewPiece();
       });

        // When going from gameplay to main menu, reset everything and set to inactive.
        registrar.RegisterStateChangeHandler(State.Gameplay, State.MainMenu, () =>
        {
            ResetEverything();
            this.gameObject.SetActive(false);
        });
    }

    public List<GameObject> PiecePrefabs;

    public Text ScoreText;
    public Text LevelText;
    public Text LinesText;

    [HideInInspector]
    public List<GameObject> FallenTiles
    {
        get
        {
            var result = new List<GameObject>();
            for (int x = 0; x < _fallenTileGrid.GetLength(0); x++)
            {
                for (int y = 0; y < _fallenTileGrid.GetLength(1); y++)
                {
                    if (_fallenTileGrid[x, y] != null)
                    {
                        result.Add(_fallenTileGrid[x, y]);
                    }
                }
            }
            return result;
        }
    }

    private GameObject[,] _fallenTileGrid;
    private List<GameObject> _randomPrefabsBag;
    private GameObject[] _nextPieces;

    // IDK if there's a better way than tracking the prefabs? But makes it so we
    // can swap back and forth between falling piece and hold piece.
    private GameObject _fallingPiece;
    private GameObject _fallingPiecePrefab;
    private GameObject _holdPiece;
    private GameObject _holdPiecePrefab;

    // Lets us make sure the user can't hold a piece back and forth multiple
    // times.
    private bool _hasPieceBeenHeld;

    public float DropTimeForCurrentLevel
    {
        get { return _dropTimeForCurrentLevel; }
    }
    // Initialize to drop time for level 1.
    private float _dropTimeForCurrentLevel = Constants.DROP_TIME_FOR_LEVEL(1);

    private int _score = 0;
    public int Score
    {
        get { return _score; }
        set { _score = value; ScoreText.text = _score.ToString(); }
    }
    private int _level = 1;
    public int Level
    {
        get { return _level; }
        set
        {
            if (_level != value) _dropTimeForCurrentLevel = Constants.DROP_TIME_FOR_LEVEL(value);
            LevelText.text = value.ToString();

            _level = value;
        }
    }
    private int _lines = 0;
    public int Lines
    {
        get { return _lines; }
        set { _lines = value; LinesText.text = _lines.ToString(); }
    }

    void Awake()
    {
        _fallenTileGrid = new GameObject[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
        _randomPrefabsBag = new List<GameObject>(PiecePrefabs.Count);
        _nextPieces = new GameObject[Constants.NEXT_PIECES_COUNT];

        // Make sure everything is turned off.
        ResetEverything();
    }

    void Start()
    {
        this.gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        // Hold piece.
        if (GameInput.GetControlDown(Control.Hold))
        {
            TryHoldPiece();
        }

        // Quit to main menu.
        if (GameInput.GetControlDown(Control.Quit))
        {
            FindObjectOfType<GameManager>().GoToState(State.MainMenu);
        }
    }

    // If prefabToSpawn is null, will pull from the random bag.
    private void SpawnNewPiece(GameObject prefabToSpawn = null)
    {
        // If random bag doesn't have enough pieces, re-seed.
        if (_randomPrefabsBag.Count < Constants.NEXT_PIECES_COUNT + 1)
        {
            var bagIndices = Enumerable.Range(0, PiecePrefabs.Count).OrderBy(i => Random.value);
            foreach (int bagIndex in bagIndices)
            {
                _randomPrefabsBag.Add(PiecePrefabs[bagIndex]);
            }
        }

        // Grab next piece from the bag, if no prefab was passed in.
        if (prefabToSpawn == null)
        {
            prefabToSpawn = _randomPrefabsBag[0];
            _randomPrefabsBag.RemoveAt(0);

            // Update next pieces.
            for (int i = 0; i < Constants.NEXT_PIECES_COUNT; i++)
            {
                if (_nextPieces[i] != null) Destroy(_nextPieces[i]);
                _nextPieces[i] = Instantiate(_randomPrefabsBag[i],
                                new Vector2(15, -3.75f - (2.75f * i)) + _randomPrefabsBag[i].GetComponent<PieceData>().VisualCenterOffset,
                                Quaternion.identity,
                                this.transform);
                _nextPieces[i].name = $"NextPiece{i + 1}";
            }
        }

        // Actually spawn the piece! This is like the whole point of the function.
        PieceData pieceData = prefabToSpawn.GetComponent<PieceData>();
        _fallingPiecePrefab = prefabToSpawn;
        _fallingPiece = Instantiate(prefabToSpawn,
                            new Vector2(Constants.BOARD_WIDTH / 2 - 1, 0) + pieceData.SpawnPositionOffset,
                            Quaternion.identity,
                            this.transform);
        _fallingPiece.name = $"FallingPiece";
        _fallingPiece.AddComponent<FallingPiece>();
        _fallingPiece.GetComponent<FallingPiece>().PieceData = pieceData;

        // User is now allowed to hold piece again.
        _hasPieceBeenHeld = false;
    }

    public void AddFallingPieceToFallenTiles()
    {
        foreach (SpriteRenderer pieceTile in _fallingPiece.GetComponent<FallingPiece>().GetChildSprites())
        {
            var x = Mathf.RoundToInt(pieceTile.transform.position.x);
            var y = Mathf.RoundToInt(pieceTile.transform.position.y);
            // TODO: These tiles sometimes have weird itty-bitty offsets applied to their position?
            // This makes collision detection not work, so the falling piece goes right through the already-fallen tiles.
            var newTile = new GameObject($"FallenTile[{x},{y}]", typeof(SpriteRenderer));
            newTile.transform.position = new Vector3(x, y, 0);
            newTile.transform.parent = this.transform;
            newTile.transform.rotation = pieceTile.transform.rotation;
            var spriteRenderer = newTile.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = pieceTile.sprite;
            _fallenTileGrid[x, -y] = newTile;
            // Debug.Log($"Adding new fallen tile at [{x}, {y}] with position {newTile.transform.position}");
        }
        Destroy(_fallingPiece);
        _fallingPiece = null;
        _fallingPiecePrefab = null;

        // Check each line to see whether it's full.
        var fullLineYCoordinates = new List<int>();
        for (int y = 0; y < _fallenTileGrid.GetLength(1); y++)
        {
            bool lineHasHoles = false;
            for (int x = 0; x < _fallenTileGrid.GetLength(0); x++)
            {
                if (_fallenTileGrid[x, y] == null) lineHasHoles = true;
            }

            if (!lineHasHoles) fullLineYCoordinates.Add(y);
        }

        Lines += fullLineYCoordinates.Count;
        Level = (Lines / 10) + 1;

        // Remove any full lines, move other lines down, and spawn a new piece,
        StartCoroutine(RemoveLinesAndSpawnPiece(fullLineYCoordinates));
    }

    // This is a co-routine because the flashing needs to happen over multiple frames.
    IEnumerator RemoveLinesAndSpawnPiece(List<int> yCoordinates)
    {
        if (yCoordinates.Count > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                // foreach (SpriteRenderer piece in pieces)
                // {
                //     // TODO: flash on.
                // }
                // yield return new WaitForSeconds(0.125f);

                // foreach (SpriteRenderer piece in pieces)
                // {
                //     // TODO: flash off.
                // }
                // yield return new WaitForSeconds(0.125f);
            }
        }

        // After flashing lines, remove them, drop other lines, and spawn a new piece.
        yCoordinates.Sort();
        yCoordinates.Reverse();
        for (int yCoordinateIndex = 0; yCoordinateIndex < yCoordinates.Count; yCoordinateIndex++)
        {
            var yCoordinate = yCoordinates[yCoordinateIndex];

            // Remove this line.
            for (int x = 0; x < _fallenTileGrid.GetLength(0); x++)
            {
                Destroy(_fallenTileGrid[x, yCoordinate]);
                _fallenTileGrid[x, yCoordinate] = null;
            }

            // Move all lines above it down.
            for (int yAbove = yCoordinate; yAbove >= 0; yAbove--)
            {
                for (int x = 0; x < _fallenTileGrid.GetLength(0); x++)
                {
                    if (yAbove == 0)
                    {
                        // Top line just becomes empty.
                        _fallenTileGrid[x, yAbove] = null;
                    }
                    else
                    {
                        // Other lines get the stuff above it.
                        _fallenTileGrid[x, yAbove] = _fallenTileGrid[x, yAbove - 1];
                        if (_fallenTileGrid[x, yAbove] != null)
                        {
                            _fallenTileGrid[x, yAbove].transform.position += new Vector3(0, -1, 0);
                        }
                    }
                }
            }

            // Since we've moved lines down, need to update next yCoordinates as well.
            for (int otherYCoordinateIndex = yCoordinateIndex + 1; otherYCoordinateIndex < yCoordinates.Count; otherYCoordinateIndex++)
            {
                yCoordinates[otherYCoordinateIndex]++;
            }
        }

        SpawnNewPiece();

        yield return null;
    }

    // Tries to hold the current piece.
    // May not be able to, for example if the user has already held the
    // current piece.
    public void TryHoldPiece()
    {
        if (_hasPieceBeenHeld) return;

        GameObject oldHoldPiecePrefab = _holdPiecePrefab;

        // Destroy old hold piece.
        Destroy(_holdPiece);
        _holdPiece = null;

        // Set up hold piece.
        _holdPiecePrefab = _fallingPiecePrefab;
        _holdPiece = Instantiate(_holdPiecePrefab,
                                 new Vector3(-7f, -3.75f, 0) + (Vector3)_holdPiecePrefab.GetComponent<PieceData>().VisualCenterOffset,
                                 Quaternion.identity,
                                 this.transform);
        _holdPiece.name = "HoldPiece";

        // Destroy old falling piece.
        Destroy(_fallingPiece);
        _fallingPiece = null;
        _fallingPiecePrefab = null;

        // Set up new falling piece.
        // If there was no piece previously held, this will grab a new piece
        // from the random bag.
        SpawnNewPiece(oldHoldPiecePrefab);

        // Make sure user can't re-hold until after they put the current piece
        // down.
        _hasPieceBeenHeld = true;
    }

    private void ResetEverything()
    {
        Score = 0;
        Level = 1;
        Lines = 0;

        // Clear fallen tiles.
        for (int x = 0; x < _fallenTileGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _fallenTileGrid.GetLength(1); y++)
            {
                if (_fallenTileGrid[x, y] != null) Destroy(_fallenTileGrid[x, y]);
                _fallenTileGrid[x, y] = null;

            }
        }

        // Clear falling piece and hold pieces.
        _randomPrefabsBag.Clear();
        for (int i = 0; i < _nextPieces.Length; i++)
        {
            if (_nextPieces[i] != null) Destroy(_nextPieces[i]);
            _nextPieces[i] = null;

        }

        if (_fallingPiece != null) Destroy(_fallingPiece);
        _fallingPiece = null;
        _fallingPiecePrefab = null;
        if (_holdPiece != null) Destroy(_holdPiece);
        _holdPiece = null;
        _holdPiecePrefab = null;
        _hasPieceBeenHeld = false;
    }

}
