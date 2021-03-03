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


    private void Awake()
    {
        Board = new Checker[_boardSize, _boardSize];
    }

    private void Start()
    {
        GenerateBoard();
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