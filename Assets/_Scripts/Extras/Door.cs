using UnityEngine;

public class Door : MonoBehaviour, IInteractableAction
{
    [SerializeField] private Transform _openPosition;
    [SerializeField] private Transform _closedPosition;
    [SerializeField] private float _speed = 2f;

    private bool _isOpen;

    void Update()
    {
        Transform target = _isOpen ? _openPosition : _closedPosition;
        transform.position = Vector3.Lerp(transform.position, target.position, _speed * Time.deltaTime);
    }

    public void Activate()
    {
        _isOpen = true;
    }

    public void Deactivate()
    {
        _isOpen = false;
    }
}