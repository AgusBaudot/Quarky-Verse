using System.Collections;
using UnityEngine;
using UnityEngine.Events;

//If this is supposed to be interacted with, it should implement the IInteractable interface. Otherwise, the ObjectDetectionSystem won't find it.
public class ActionButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private bool _temporaryMode = false;
    [SerializeField] private float _activeDuration = 3f;

    [Header("Events")]
    [SerializeField] private UnityEvent _onPressed;
    [SerializeField] private UnityEvent _onReleased;

    [Header("Visual")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _inactiveColor = Color.red;
    [SerializeField] private Color _activeColor = Color.green;

    private bool _isActive;

    private void Start()
    {
        UpdateVisual();
    }

    public void OnInteract()
    {
        if (_isActive) return;
        ActivateButton();
    }

    private void ActivateButton()
    {
        _isActive = true;
        Debug.Log("Button Pressed");
        _onPressed?.Invoke();
        UpdateVisual();
        if (_temporaryMode) StartCoroutine(TemporaryRoutine());
    }

    private IEnumerator TemporaryRoutine()
    {
        yield return new WaitForSeconds(_activeDuration);
        DeactivateButton();
    }

    private void DeactivateButton()
    {
        _isActive = false;
        Debug.Log("Button Released");
        _onReleased?.Invoke();
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (_renderer != null)
        {
            Color target = _isActive ? _activeColor : _inactiveColor;
            _renderer.material.SetColor("_BaseColor", target);
        }
    }
}