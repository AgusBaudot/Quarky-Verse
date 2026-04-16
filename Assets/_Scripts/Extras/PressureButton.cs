using UnityEngine;

public class PressureButton : MonoBehaviour
{
    [SerializeField] private MonoBehaviour _actionTarget;

    private IInteractableAction _action;
    private int _objectsOnTop = 0;

    void Awake()
    {
        _action = _actionTarget as IInteractableAction;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (IsValid(other))
        {
            _objectsOnTop++;
            if (_objectsOnTop == 1) _action?.Activate();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (IsValid(other))
        {
            _objectsOnTop--;

            if (_objectsOnTop <= 0)
            { 
                _objectsOnTop = 0;
                _action?.Deactivate();
            }
        }
    }
    private bool IsValid(Collider other)
    {
        return other.CompareTag("Player") || other.GetComponent<PickableObject>() != null;
    }
}
