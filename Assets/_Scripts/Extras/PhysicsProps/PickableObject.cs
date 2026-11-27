using UnityEngine;

public class PickableObject : MonoBehaviour, IInteractable
{
    private Rigidbody _rb;
    public Rigidbody Rigidbody => _rb;
    [Header("Visual Feedback")]
    [SerializeField] private Renderer _renderer;
    private Collider _collider;
    public Collider Collider => _collider;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void OnGrab()
    {
        if ( _rb != null ) 
        { 
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.freezeRotation = true;
        }
    }

    public void OnRelease()
    {
        if (_rb != null)
        {
            _rb.useGravity = true;
            _rb.freezeRotation = false;
        }
    }
    // INTERACTION SYSTEM
    // =========================

    public void OnFocus()
    {
        if (_renderer != null) _renderer.material.EnableKeyword("_EMISSION");
    }

    public void OnLoseFocus()
    {
        if (_renderer != null) _renderer.material.DisableKeyword("_EMISSION");
    }

    public void OnInteract()
    {
        Debug.Log("Objeto interactuable detectado");
    }
}
