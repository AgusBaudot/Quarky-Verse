using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableObject : MonoBehaviour
{
    private Rigidbody _rb;
    [Header("Push Settings")]
    [SerializeField] private float _moveSpeed = 3f;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // IMPORTANTE para objetos pesados
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void Push(Vector3 direction)
    {
        Vector3 velocity = direction * _moveSpeed;
        velocity.y = _rb.velocity.y;
        _rb.velocity = velocity;
    }

    public void StopPush()
    {
        _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
    }
}