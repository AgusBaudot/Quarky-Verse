using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform _openPosition;
    [SerializeField] private float _speed = 3f;
    private Vector3 _closedPosition;
    private bool _isOpen;

    void Start()
    {
        _closedPosition = transform.position;
    }

    void Update()
    {
        Vector3 target = _isOpen ? _openPosition : _closedPosition;
        transform.position = Vector3.Lerp(transform.position, target, _speed * Time.deltaTime);
    }

    public void Open()
    {
        _isOpen = true;
    }

    public void Close()
    {
        _isOpen = false;
    }
}