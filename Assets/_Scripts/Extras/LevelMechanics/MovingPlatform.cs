using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private Transform _platform;
    [SerializeField] private Vector3 _moveOffset = new Vector3(0f, 5f, 0f);
    [SerializeField] private float _speed = 2f;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private Coroutine _moveRoutine;

    private void Start()
    {
        _startPosition = _platform.position;
        _targetPosition = _startPosition + _moveOffset;
    }

    public void Activate()
    {
        MovePlatform(_targetPosition);
    }

    public void Deactivate()
    {
        MovePlatform(_startPosition);
    }

    private void MovePlatform(Vector3 target)
    {
        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Vector3 target)
    {
        while (Vector3.Distance(_platform.position, target) > 0.01f)
        {
            _platform.position = Vector3.MoveTowards(
                _platform.position,
                target,
                _speed * Time.deltaTime
            );

            yield return null;
        }
        _platform.position = target;
    }
}