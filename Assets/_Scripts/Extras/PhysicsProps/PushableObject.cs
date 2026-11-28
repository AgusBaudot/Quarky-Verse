using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PushableObject : MonoBehaviour
{
    [Header("Push Settings")]
    [SerializeField] private float _interactionRange = 1.5f;
    [SerializeField] private float _pushSpeed = 3f;

    private Rigidbody _rb;
    private Transform _activeEntity;

    public Transform ActiveEntity => _activeEntity;
    public Rigidbody ObjectRigidbody => _rb;
    public float InteractionRange => _interactionRange;
    public float PushSpeed => _pushSpeed;
    public bool IsPushing { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints |= RigidbodyConstraints.FreezeRotation;
    }

    public bool IsWithinRange(Transform entity)
    {
        if (entity == null) return false;
        Vector3 entityPosition = entity.position;
        Vector3 objectPosition = transform.position;
        entityPosition.y = 0f;
        objectPosition.y = 0f;
        return Vector3.Distance(entityPosition, objectPosition) <= _interactionRange;
    }

    public void Push(Transform activeEntity, Vector3 direction)
    {
        if (activeEntity == null || !IsWithinRange(activeEntity))
        {
            StopPush();
            return;
        }
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            StopPush();
            return;
        }
        _activeEntity = activeEntity;
        IsPushing = true;
        Vector3 pushDirection = direction.normalized;
        _rb.velocity = new Vector3(pushDirection.x * _pushSpeed,_rb.velocity.y,pushDirection.z * _pushSpeed);
    }

    public void StopPush()
    {
        IsPushing = false;
        _activeEntity = null;
        if (_rb != null) _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
    }
}