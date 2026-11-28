using UnityEngine;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    [SerializeField] private GameObject _targetDoor;
    [SerializeField] private float _interactionRange = 2f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onPressed;

    [Header("Visual")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _inactiveColor = Color.red;
    [SerializeField] private Color _activeColor = Color.green;

    public float InteractionRange => _interactionRange;
    public bool IsActivated { get; private set; }

    private void Start()
    {
        UpdateVisual();
    }

    public void OnInteract()
    {
        if (IsActivated) return;
        if (_targetDoor == null)
        {
            Debug.LogWarning($"No hay una puerta asignada al botón {name}.", this);
            return;
        }
        IsActivated = true;
        Destroy(_targetDoor);
        _targetDoor = null;
        _onPressed?.Invoke();
        UpdateVisual();
    }

    public void OnFocus() { }
    public void OnLoseFocus() { }
    private void UpdateVisual()
    {
        if (_renderer == null) return;
        Color targetColor = IsActivated ? _activeColor : _inactiveColor;
        _renderer.material.SetColor("_BaseColor", targetColor);
    }
}