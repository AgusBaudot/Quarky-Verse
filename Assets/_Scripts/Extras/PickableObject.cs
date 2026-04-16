using UnityEngine;

public class PickableObject : MonoBehaviour
{
    private Rigidbody _rb;
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void OnGrab()
    {
        if ( _rb != null ) 
        { 
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }
    }

    public void OnRelease()
    {
        if (_rb != null) _rb.useGravity = true;
        _rb.isKinematic = false;
    }
}
