using System.Collections;
using UnityEngine;

public class TimedButton : MonoBehaviour
{
    [SerializeField] private float _duration = 3f;
    [SerializeField] private MonoBehaviour _actionTarget;

    private IInteractableAction _action;
    private bool _isActive;

    void Awake()
    {
        _action = _actionTarget as IInteractableAction;
    }

    public void Press()
    {
        if (_isActive) return;
        StartCoroutine(ActivateRoutine());
    }
    private IEnumerator ActivateRoutine()
    {
        _isActive = true;
        _action?.Activate();
        yield return new WaitForSeconds(_duration);
        _action?.Deactivate();
        _isActive = false;
    }
}
