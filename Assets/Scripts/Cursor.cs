using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    [Header("Cursor Settings")]
    [SerializeField] private GameObject _cursor;
    [SerializeField] private Transform _columnsPositionsContainer;
    private bool _isAnimating = false;


    public void SetupCursorPosition() => _cursor.transform.position = new Vector3(  _columnsPositionsContainer.GetChild(0).position.x, 
                                                                                            _cursor.transform.position.y, 
                                                                                            _cursor.transform.position.z);

    public float ReturnCursorEndPosition(int column)
    {
        for (int i = 0; i < _columnsPositionsContainer.childCount; i++)
        {
            if (i == column) return _columnsPositionsContainer.GetChild(i).position.x;
        }

        return 0;
    }

    public Transform ReturnColumnsContainer() => _columnsPositionsContainer;
    public GameObject ReturnCursor() => _cursor;
    public bool ReturnIsAnimating() => _isAnimating;

    public void ChangeCursorColor(Color participantColor) => _cursor.transform.GetChild(0).GetComponent<Image>().color = participantColor;

    public IEnumerator MoveCursorToColumn(GameObject participant, int column, float duration = 0.15f)
    {
        _isAnimating = true;
        float time = 0;

        float cursorStartPos = ReturnCursor().transform.position.x;
        float cursorEndPos = ReturnCursorEndPosition(column);

        Quaternion participantStartRot = participant.transform.rotation;
        Vector3 lookDir = new Vector3(cursorEndPos, ReturnCursor().transform.position.y,ReturnCursor().transform.position.z) - participant.transform.position;
        lookDir.y = 0;
        Quaternion participantEndRot = Quaternion.LookRotation(lookDir);

        while (time < duration)
        {
            float cursorPosition = ReturnCursor().transform.position.x;
            cursorPosition = Mathf.Lerp(cursorStartPos, cursorEndPos, time / duration);
            ReturnCursor().transform.position = new Vector3(cursorPosition, ReturnCursor().transform.position.y, _cursor.transform.position.z);

            participant.transform.rotation = Quaternion.Slerp(participantStartRot, participantEndRot, time / duration);

            time += Time.deltaTime;

            yield return null;
        }

        ReturnCursor().transform.position = new Vector3(cursorEndPos, ReturnCursor().transform.position.y, _cursor.transform.position.z);
        participant.transform.rotation = participantEndRot;

        _isAnimating = false;
    }

}
