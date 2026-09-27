using UnityEngine;

public class MovingLaser : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveDistance = 3f;
    [SerializeField] private float _moveSpeed = 2f;

    private Vector3 _topPosition;
    private Vector3 _bottomPosition;

    private void Start()
    {
        _topPosition = transform.localPosition;
        _bottomPosition = _topPosition + Vector3.down * _moveDistance;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * _moveSpeed) + 1f) * 0.5f;
        transform.localPosition = Vector3.Lerp(_topPosition, _bottomPosition, t);
    }
}