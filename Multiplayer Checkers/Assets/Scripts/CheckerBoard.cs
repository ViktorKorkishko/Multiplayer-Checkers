using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

public class CheckerBoard : MonoBehaviour
{
    [Header("Board params")]
    public Checker[,] Board;
    
    [SerializeField] private int _boardSize;
    [SerializeField] private Vector3 _boardOffset;
    [SerializeField] private Vector3 _checkerOffset;

    [Header("Turn params")]
    public CheckerColor PlayerColor = CheckerColor.White;
    [SerializeField] private TurnBelongsTo TurnBelongsTo = TurnBelongsTo.Whites;
    [SerializeField] private bool hasKilliedThisTurn;
    [SerializeField] private List<Checker> _forcedCheckers = new List<Checker>();

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
        if (PlayerColor == CheckerColor.White)
        {
            TurnBelongsTo = TurnBelongsTo.Whites;
        }
        else
        {
            TurnBelongsTo = TurnBelongsTo.Blacks;
        }

        if (Input.GetMouseButtonDown(0))
        {
            UpdateMouseOver();
            
            SelectChecker(_mouseOver);
        }

        if (Input.GetMouseButton(0))
        {
            UpdateMouseOver();
            
            if (_selectedChecker)
            {
                UpdateCheckerDrag(_selectedChecker);
            }
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

    private void UpdateCheckerDrag(Checker checker)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25f,
            LayerMask.GetMask("Board")))
        {
            checker.transform.position = hit.point + Vector3.up;
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
            if (checker.CheckerColor == PlayerColor)
            {
                if (_forcedCheckers.Count == 0)
                {
                    _selectedChecker = checker;
                    _startDrag = _mouseOver;
                }
                else
                {
                    // look for the checker under 
                    if (_forcedCheckers.Find(fp => fp == checker) == null)
                    {
                        return;
                    }

                    _selectedChecker = checker;
                    _startDrag = _mouseOver;
                }
            }
        }
    }

    private void TryMove(Vector2Int startPosition, Vector2Int endPosition)
    {
        ScanForPossibleMove();
        
        // multiplayer support
        _startDrag = startPosition;
        _endDrag = endPosition;

        _selectedChecker = Board[startPosition.x, startPosition.y];

        // out if bounds
        if (endPosition.x < 0 || endPosition.x >= _boardSize ||
            endPosition.y < 0 || endPosition.y >= _boardSize)
        {
            if (_selectedChecker)
            {
                MoveChecker(_selectedChecker, startPosition);
            }

            _startDrag = Vector2Int.zero;
            _selectedChecker = null;
            return;
        }

        if (_selectedChecker)
        {
            //if it has not moved
            if (_endDrag == _startDrag)
            {
                MoveChecker(_selectedChecker, startPosition);
                _startDrag = Vector2Int.zero;
                _selectedChecker = null;
                return;
            }
            
            //check if it is a valid move
            if (_selectedChecker.ValidMove(Board, _startDrag, _endDrag))
            {
                // did we kill anything 
                // if this is a jump
                if (Math.Abs(endPosition.x - startPosition.x) == 2)
                {
                    Checker middleChecker = Board[(startPosition.x + endPosition.x) / 2,
                        (startPosition.y + endPosition.y) / 2];
                    if (middleChecker)
                    {
                        Board[(startPosition.x + endPosition.x) / 2,
                            (startPosition.y + endPosition.y) / 2] = null;
                        middleChecker.gameObject.SetActive(false);
                        hasKilliedThisTurn = true;
                    }
                }
                
                // were we suppoused to kill
                if (_forcedCheckers.Count != 0 && !hasKilliedThisTurn)
                {
                    MoveChecker(_selectedChecker, _startDrag);
                    _startDrag = Vector2Int.zero;
                    _selectedChecker = null;
                    return;
                }

                Board[endPosition.x, endPosition.y] = _selectedChecker;
                Board[startPosition.x, startPosition.y] = null;
                
                MoveChecker(_selectedChecker, endPosition);

                EndTurn();
            }
            else
            {
                MoveChecker(_selectedChecker, _startDrag);
                _startDrag = Vector2Int.zero;
                _selectedChecker = null;
                return;
            }
        }
    }

    private void EndTurn()
    {
        Vector2Int endPosition = _endDrag;

        if (_selectedChecker)
        {
            if (_selectedChecker.CheckerColor == CheckerColor.White && _selectedChecker.CheckerType == CheckerType.Default && endPosition.y == 7)
            {
                _selectedChecker.CheckerType = CheckerType.King;
                _selectedChecker.transform.Rotate(Vector3.right * 180f);
            }
            else if (_selectedChecker.CheckerColor == CheckerColor.Black && _selectedChecker.CheckerType == CheckerType.Default && endPosition.y == 0)
            {
                _selectedChecker.CheckerType = CheckerType.King;
                _selectedChecker.transform.Rotate(Vector3.right * 180f);
            }
        }
        
        _selectedChecker = null;
        _startDrag = Vector2Int.zero;

        ScanForPossibleMove(_selectedChecker, endPosition);
        if (_forcedCheckers.Count != 0 && hasKilliedThisTurn)
        {
            return;
        }
        
        if (TurnBelongsTo == TurnBelongsTo.Whites)
        {
            TurnBelongsTo = TurnBelongsTo.Blacks;
        }
        else
        {
            TurnBelongsTo = TurnBelongsTo.Whites;
        }
        
        if (PlayerColor == CheckerColor.Black)
        {
            PlayerColor = CheckerColor.White;
        }
        else
        {
            PlayerColor = CheckerColor.Black;
        }
        
        hasKilliedThisTurn = false;
        CheckVictory();
    }

    private void CheckVictory()
    {
        var checkers = FindObjectsOfType<Checker>();
        bool hasWhite = false, hasBlack = true;

        for (int i = 0; i < checkers.Length; i++)
        {
            if (checkers[i].CheckerColor == CheckerColor.White)
            {
                hasWhite = true;
            }
            else
            {
                hasBlack = true;
            }
        }

        if (!hasWhite)
        {
            Victory(false);
        }

        if (!hasBlack)
        {
            Victory(true);
        }
    }

    private void Victory(bool isWhite)
    {
        if (isWhite)
        {
            Debug.Log("White team has won");
        }
        else
        {
            Debug.Log("Black team has won");
        }
    }

    private void ScanForPossibleMove(Checker checker, Vector2Int position)
    {
        _forcedCheckers.Clear();

        if (Board[position.x, position.y].IsForcedToMove(Board, position))
        {
            _forcedCheckers.Add(_selectedChecker);
        }
    }

    private void ScanForPossibleMove()
    {
        _forcedCheckers.Clear();

        for (int y = 0; y < _boardSize; y++)
        {
            for (int x = 0; x < _boardSize; x++)
            {
                if (Board[x, y] && Board[x, y].CheckerColor == CheckerColor.White && TurnBelongsTo == TurnBelongsTo.Whites)
                {
                    if (Board[x, y].IsForcedToMove(Board, new Vector2Int(x, y)))
                    {
                        _forcedCheckers.Add(Board[x, y]);
                    }
                }
            }
        }
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