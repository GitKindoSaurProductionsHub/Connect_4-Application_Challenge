using System;
using System.Collections.Generic;
using UnityEngine;

public enum TurnFor
{
    None = 0,
    Player = 1,
    Opponent = 2
}

public class Game : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private Transform _gridPositionsContainer;
    [SerializeField] private Transform _discPositionsContainer;
    [SerializeField] private GameObject _discPrefab;
    private bool _gameOver = false;
    private int[,] _board;

    [Serializable]
    struct Grid
    {
        public int columns;
        public int rows;
        public Vector3 startPosition;
        public Vector3 positionOffset;
    };
    [SerializeField] private Grid _boardSettings;

    [Header("Participant Settings")]
    [SerializeField] private PlayerEntity _playerEntity;
    [SerializeField] private OpponentEntity _opponentEntity;
    private TurnFor _turnIndicator = TurnFor.None;
    private List<GameObject[]> _columnList = new List<GameObject[]>();

    void Start()
    {
        SetupBoard(_boardSettings);
    }

    private void Update()
    {
        if (!_gameOver)
        {
            if (_turnIndicator == TurnFor.Player) _playerEntity.HandleInput();
            else if (_turnIndicator == TurnFor.Opponent) _opponentEntity.HandleInput();
        }
    }

    private void SetupBoard(Grid board)
    {
        _board = new int[board.rows, board.columns];
        int discID = 0;

        for (int column = 0; column < board.columns; column++)
        {
            GameObject[] columnGroup = new GameObject[board.rows];
            for (int row = 0; row < board.rows; row++)
            {
                GameObject spot = new GameObject();
                spot.transform.SetParent(_gridPositionsContainer);
                spot.transform.localPosition = board.startPosition + new Vector3(board.positionOffset.x * column,
                                                board.positionOffset.y * row,
                                                0);
                spot.name = $"P{discID}";

                discID++;
                columnGroup[row] = spot;
                _board[row, column] = (int)TurnFor.None;
            }
            _columnList.Add(columnGroup);
        }

        SetTurn(TurnFor.Player);
    }

    public void PerformTurnAction(int column)
    {
        int spotsTaken = 0;

        for (int i = 0; i < _columnList[column].Length; i++)
        {
            if (CombinedOccupiedSpots().Contains(_columnList[column][i]))
            {
                spotsTaken += 1;
                if (spotsTaken == _columnList[column].Length) return;
            }
            else
            {
                CreateDisc(_columnList[column][i], i, column);

                if (CheckIfGameOver()) return;

                SetTurn(_turnIndicator == TurnFor.Player ? TurnFor.Opponent : TurnFor.Player);

                break;
            }
        }
    }

    private void SetTurn(TurnFor giveTurnTo)
    {
        _turnIndicator = giveTurnTo;

        _opponentEntity.AlterTurnIndicator(giveTurnTo != TurnFor.Player);
        _playerEntity.AlterTurnIndicator(giveTurnTo == TurnFor.Player);
    }

    private List<GameObject> CombinedOccupiedSpots()
    {
        List<GameObject> occupiedSpots = new List<GameObject>();
        occupiedSpots.AddRange(_playerEntity.ReturnOccupiedSpots());
        occupiedSpots.AddRange(_opponentEntity.ReturnOccupiedSpots());
        return occupiedSpots;
    }

    private void CreateDisc(GameObject columnSpot, int row, int column)
    {
        GameObject disc = Instantiate(_discPrefab, columnSpot.transform.position, Quaternion.identity, _discPositionsContainer);
        disc.GetComponent<MeshRenderer>().material.color = _turnIndicator == TurnFor.Player ? _playerEntity.ReturnParticipantColor() : _opponentEntity.ReturnParticipantColor();

        if (_turnIndicator == TurnFor.Player)
        {
            _playerEntity.UpdateOccupiedSpots(columnSpot);
            _board[row, column] = (int)TurnFor.Player;
        }
        else
        {
            _opponentEntity.UpdateOccupiedSpots(columnSpot);
            _board[row, column] = (int)TurnFor.Opponent;
        }
    }

    private bool CheckIfGameOver() => CheckDraw() || CheckHorizontal((int)_turnIndicator) || CheckVertical((int)_turnIndicator) || CheckDiagonal((int)_turnIndicator);

    public bool ReturnIFGameOver() => _gameOver;

    public TurnFor ReturnWhoseTurn() => _turnIndicator;

    private bool CheckHorizontal(int player)
    {
        for (int row = 0; row < _boardSettings.rows; row++)
        {
            for (int col = 0; col < _boardSettings.columns - 3; col++)
            {
                if (_board[row, col] == player && _board[row, col + 1] == player &&
                    _board[row, col + 2] == player && _board[row, col + 3] == player)
                {
                    Debug.Log("Horizontal Win");
                    _gameOver = true;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckVertical(int player)
    {
        for (int col = 0; col < _boardSettings.columns; col++)
        {
            for (int row = 0; row < _boardSettings.rows - 3; row++)
            {
                if (_board[row, col] == player && _board[row + 1, col] == player &&
                    _board[row + 2, col] == player && _board[row + 3, col] == player)
                {
                    Debug.Log("Vertical Win");
                    _gameOver = true;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckDiagonal(int player)
    {
        for (int row = 0; row < _boardSettings.rows - 3; row++)
        {
            for (int col = 0; col < _boardSettings.columns - 3; col++)
            {
                if (_board[row, col] == player && _board[row + 1, col + 1] == player &&
                    _board[row + 2, col + 2] == player && _board[row + 3, col + 3] == player)
                {
                    Debug.Log("Positive Diagonal Win");
                    _gameOver = true;
                    return true;
                }
            }
        }

        for (int row = 3; row < _boardSettings.rows; row++)
        {
            for (int col = 0; col < _boardSettings.columns - 3; col++)
            {
                if (_board[row, col] == player && _board[row - 1, col + 1] == player &&
                    _board[row - 2, col + 2] == player && _board[row - 3, col + 3] == player)
                {
                    Debug.Log("Negative Diagonal Win");
                    _gameOver = true;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckDraw()
    {
        if(CombinedOccupiedSpots().Count == _boardSettings.rows * _boardSettings.columns)
        {
            Debug.Log("ITS A DRAW ... HOW?!");
            return true;
        }

        return false;
    }
}
