using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity : MonoBehaviour
{
    [Header("Entity Settings")]
    [SerializeField] private Color _participantColor;
    [SerializeField] private GameObject _participantTurnIndicator;
    private List<GameObject> _occupiedSpots = new List<GameObject>();

    [Header("SFX Settings")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("Other Settings")]
    [SerializeField] private Game _game;
    [SerializeField] private Cursor _cursor;
    private int _currentSelectedColumn = 0;

    void Start()
    {
        _cursor.SetupCursorPosition();
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Return)) _game.PerformTurnAction(_currentSelectedColumn);

        if (Input.GetKeyDown(KeyCode.A) | Input.GetKeyDown(KeyCode.D) && !_cursor.ReturnIsAnimating())
        {
            if (Input.GetKeyDown(KeyCode.A) && _currentSelectedColumn > 0)
            {
                _sfxSource.Play();
                _currentSelectedColumn -= 1;
            }
            else if (Input.GetKeyDown(KeyCode.D) && _currentSelectedColumn < _cursor.ReturnColumnsContainer().childCount - 1)
            {
                _sfxSource.Play();
                _currentSelectedColumn += 1;
            }

            _sfxSource.Play();
            StartCoroutine(_cursor.MoveCursorToColumn(this.gameObject, _currentSelectedColumn, _sfxSource.clip.length));
        }
    }

    public void UpdateOccupiedSpots(GameObject spot) => _occupiedSpots.Add(spot);

    public List<GameObject> ReturnOccupiedSpots() => _occupiedSpots;

    public void AlterTurnIndicator(bool isNextTurn)
    {
        _participantTurnIndicator.SetActive(isNextTurn);
        if (!isNextTurn) return;
        _cursor.ChangeCursorColor(ReturnParticipantColor());
        StartCoroutine(_cursor.MoveCursorToColumn(this.gameObject, _currentSelectedColumn));
    }

    public Color ReturnParticipantColor() => _participantColor;

    public int ReturnLastSelectedColumn() => _currentSelectedColumn;
}
