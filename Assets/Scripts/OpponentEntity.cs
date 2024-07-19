using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpponentEntity : MonoBehaviour
{
    [Serializable]
    struct MinMaxFloat
    {
        public float minf, maxf;
    };

    [Serializable]
    struct MinMaxInt
    {
        public int mini, maxi;
    };

    [Header("Entity Settings")]
    [SerializeField] private TurnFor _participantType = TurnFor.None;
    [SerializeField] private Color _participantColor;
    [SerializeField] private GameObject _participantTurnIndicator;
    private List<GameObject> _occupiedSpots = new List<GameObject>();

    [Header("AI Settings")]
    [SerializeField] private bool _AIMode = false;
    [SerializeField, Range(0.0f, 1.0f)] private float _cheatChance;
    [SerializeField] private MinMaxFloat _minMaxDecisionTime;
    [SerializeField] private float _maxWaitTillPerformAction;
    [SerializeField] private MinMaxInt _minMaxDecisionSteps;
    [SerializeField] private PlayerEntity _player;
    private bool _isRunning = false;

    [Header("SFX Settings")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("Other Settings")]
    [SerializeField] private Game _game;
    [SerializeField] private Cursor _cursor;
    private int _currentSelectedColumn = 0;

    public void HandleInput()
    {
        if (!_AIMode)
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

                StartCoroutine(_cursor.MoveCursorToColumn(this.gameObject, _currentSelectedColumn, _sfxSource.clip.length));
            }
        }
        else
        {
            if (_isRunning) return;
            StartCoroutine(AI());
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

    private int ReturnCheatOffset(float chance)
    {
        if (chance <= 0.4f) return -1;
        else if (chance > 0.4f && chance <= 0.8f) return 1;

        return 0;
    }

    private IEnumerator AI()
    {
        _isRunning = true;

        float decisionTime = UnityEngine.Random.Range(_minMaxDecisionTime.minf, _minMaxDecisionTime.maxf);
        int steps = UnityEngine.Random.Range(_minMaxDecisionSteps.mini, _minMaxDecisionSteps.maxi);
        int previousSelectedColumn = _currentSelectedColumn;

        while (true)
        {
            decisionTime -= Time.deltaTime;

            if (decisionTime <= 0)
            {
                previousSelectedColumn = _currentSelectedColumn;
                _currentSelectedColumn = UnityEngine.Random.Range(0.0f, 1.0f) <= _cheatChance ?
                                         _player.ReturnLastSelectedColumn() + ReturnCheatOffset(UnityEngine.Random.Range(0.0f, 1.0f)) : UnityEngine.Random.Range(0, 7);
                
                while (_currentSelectedColumn == previousSelectedColumn)
                {
                    _currentSelectedColumn = UnityEngine.Random.Range(0.0f, 1.0f) <= _cheatChance ?
                                         _player.ReturnLastSelectedColumn() + ReturnCheatOffset(UnityEngine.Random.Range(0.0f, 1.0f)) : UnityEngine.Random.Range(0, 7);
                }

                _sfxSource.Play();
                StartCoroutine(_cursor.MoveCursorToColumn(this.gameObject, _currentSelectedColumn, _sfxSource.clip.length));

                yield return new WaitForSeconds(_sfxSource.clip.length);

                if (steps > 0)
                {
                    decisionTime = UnityEngine.Random.Range(_minMaxDecisionTime.minf, _minMaxDecisionTime.maxf);
                    steps -= 1;
                }
                else
                {
                    _game.PerformTurnAction(_currentSelectedColumn);
                    break;
                }
            }
            yield return null;
        }

        _isRunning = false;
    }

    public Color ReturnParticipantColor() => _participantColor;
}
