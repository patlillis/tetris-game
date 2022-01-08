using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> PiecePrefabs;
    public GameObject TilePrefab;

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
        set { _level = value; LevelText.text = _level.ToString(); }
    }
    private int _lines = 0;
    public int Lines
    {
        get { return _lines; }
        set { _lines = value; LinesText.text = _lines.ToString(); }
    }

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        _fallenTileGrid = new GameObject[Constants.BOARD_WIDTH, Constants.BOARD_HEIGHT];
        _randomPrefabsBag = new List<GameObject>(PiecePrefabs.Count);
        _nextPieces = new GameObject[Constants.NEXT_PIECES_COUNT];
        SpawnNewPiece();
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void SpawnNewPiece()
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

        // Spawn next piece in the bag.
        GameObject piecePrefab = _randomPrefabsBag[0];
        _randomPrefabsBag.RemoveAt(0);
        TetrisPiece pieceScript = piecePrefab.GetComponent<TetrisPiece>();
        Instantiate(piecePrefab,
                            new Vector2(Constants.BOARD_WIDTH / 2 - 1, 0) + pieceScript.SpawnPositionOffset,
                            Quaternion.identity,
                            this.transform);

        Debug.Log("a");
        Debug.Log("b");

        // Update next pieces.
        for (int i = 0; i < Constants.NEXT_PIECES_COUNT; i++)
        {
            if (_nextPieces[i] != null) Destroy(_nextPieces[i]);
            _nextPieces[i] = Instantiate(PiecePrefabs[i],
                            new Vector2(0, 0) + pieceScript.SpawnPositionOffset,
                            Quaternion.identity,
                            this.transform);
        }
        Debug.Log("c");
    }

    public void AddPieceToFallenTiles(GameObject piece)
    {
        foreach (SpriteRenderer pieceTile in piece.GetComponentsInChildren<SpriteRenderer>())
        {
            var newTile = Instantiate(TilePrefab, pieceTile.transform.position, pieceTile.transform.rotation, this.transform);
            var x = Mathf.RoundToInt(pieceTile.transform.position.x);
            var y = Mathf.RoundToInt(pieceTile.transform.position.y);
            _fallenTileGrid[x, -y] = newTile;
        }
        Destroy(piece);

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
}
