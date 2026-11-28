using UnityEngine;

public class PickableObject : MonoBehaviour, IInteractable
{
    private Rigidbody _rb;
    public Rigidbody Rigidbody => _rb;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer _renderer;

    private Collider _collider;
    public Collider Collider => _collider;

    private RigidbodyInterpolation _originalInterpolation;
    private CollisionDetectionMode _originalCollisionMode;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void OnGrab()
    {
        if (_rb != null)
        {
            _originalInterpolation = _rb.interpolation;
            _originalCollisionMode = _rb.collisionDetectionMode;

            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    public void OnRelease(float maxReleaseSpeed = 5f)
    {
        if (_rb != null)
        {
            _rb.useGravity = true;
            _rb.freezeRotation = false;
            _rb.interpolation = _originalInterpolation;
            _rb.collisionDetectionMode = _originalCollisionMode;

            // Evita que salga disparada si la soltás en medio de un volantazo
            if (_rb.velocity.magnitude > maxReleaseSpeed)
            {
                _rb.velocity = _rb.velocity.normalized * maxReleaseSpeed;
            }
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