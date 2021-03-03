using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckerBoard : MonoBehaviour
{
    [Header("Board params")] 
    public Checker[,] Board;

    [SerializeField] private int _boardSize;
    [SerializeField] private Vector3 _boardOffset;
    [SerializeField] private Vector3 _checkerOffset;
    
    [Header("Spawning params")] 
    [SerializeField] private GameObject _whiteCheckerPrefab;
    [SerializeField] private GameObject _blackCheckerPrefab;

    [Header("Input params")] 
    [SerializeField] private Checker _selectedChecker;
    [SerializeField] private Vector2Int _mouseOver;
    [SerializeField] private Vector2Int _startDrag;
    [SerializeField] private Vector2Int _endDrag;

    private void Awake()
    {
        Board = new Checker[_boardSize, _boardSize];
    }

    private void Start()
    {
        GenerateBoard();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UpdateMouseOver();
            
            SelectChecker(_mouseOver);
        }

        if (Input.GetMouseButton(0))
        {
            UpdateMouseOver();
            
        }

        if (Input.GetMouseButtonUp(0))
        {
            UpdateMouseOver();
            TryMove(_startDrag, _mouseOver);
        }
    }

    private void UpdateMouseOver()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25f,
            LayerMask.GetMask("Board")))
        {
            _mouseOver = new Vector2Int((int) (hit.point.x - _boardOffset.x), (int) (hit.point.z - _boardOffset.z));
        }
        else
        {
            _mouseOver = new Vector2Int(-1, -1);
        }
    }

    private void SelectChecker(Vector2Int position)
    {
        // if out of bounds
        if (position.x <= 0 || position.x >= _boardSize || 
            position.y <= 0 || position.y >= _boardSize)
        {
            return;
        }

        Checker checker = Board[position.x, position.y];
        if (checker)
        {
            _selectedChecker = checker;
            _startDrag = _mouseOver;
        }
    }

    private void TryMove(Vector2Int startPosition, Vector2Int endPosition)
    {
        // multiplayer support
        _startDrag = startPosition;
        _endDrag = endPosition;

        _selectedChecker = Board[startPosition.x, startPosition.y];
        
        MoveChecker(_selectedChecker, endPosition);
    }

    private void GenerateBoard()
    {
        // generate white team
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < _boardSize; x += 2)
            {
                if (y % 2 == 0)
                {
                    SpawnChecker(_whiteCheckerPrefab, new Vector2Int(x, y));
                    continue;
                }

                SpawnChecker(_whiteCheckerPrefab, new Vector2Int(x + 1, y));
            }
        }

        // generate black team
        for (int y = _boardSize - 3; y < _boardSize; y++)
        {
            for (int x = 0; x < _boardSize; x += 2)
            {
                if (y % 2 == 0)
                {
                    SpawnChecker(_blackCheckerPrefab, new Vector2Int(x, y));
                    continue;
                }

                SpawnChecker(_blackCheckerPrefab, new Vector2Int(x + 1, y));
            }
        }
    }

    private void SpawnChecker(GameObject checkerPrefab, Vector2Int position)
    {
        GameObject go = Instantiate(checkerPrefab, transform);

        Checker checker = go.GetComponent<Checker>();

        MoveChecker(checker, position);
        Board[position.x, position.y] = checker;
    }

    private void MoveChecker(Checker checker, Vector2Int newPosition)
    {
        checker.transform.position = (Vector3.right * newPosition.x) + (Vector3.forward * newPosition.y) +
                                     _boardOffset + _checkerOffset;
    }
}