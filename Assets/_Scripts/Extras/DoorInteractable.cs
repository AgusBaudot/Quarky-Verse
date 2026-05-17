using System.Collections;
using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private Transform _doorVisual;
    [SerializeField] private Vector3 _openOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float _speed = 3f;

    private Vector3 _closedPosition;
    private Vector3 _openPosition;

    private Coroutine _moveRoutine;

    private void Start()
    {
        _closedPosition = _doorVisual.position;
        _openPosition = _closedPosition + _openOffset;
    }

    public void Open()
    {
        MoveDoor(_openPosition);
    }

    public void Close()
    {
        MoveDoor(_closedPosition);
    }

    private void MoveDoor(Vector3 target)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        while (Vector3.Distance(_doorVisual.position, target) > 0.01f)
        {
            _doorVisual.position = Vector3.MoveTowards(
                _doorVisual.position,
                target,
                _speed * Time.deltaTime
            );
            yield return null;
        }
        _doorVisual.position = target;
    }
}