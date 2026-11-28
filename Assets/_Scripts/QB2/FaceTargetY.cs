using UnityEngine;

public class FaceTargetY : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 8f;

    private QB2GuideTrigger _guideTrigger;

    private void Awake()
    {
        _guideTrigger = GetComponentInParent<QB2GuideTrigger>();
    }

    private void OnEnable()
    {
        RotateTowardTarget(instant: true);
    }

    private void LateUpdate()
    {
        RotateTowardTarget(instant: false);
    }

    private void RotateTowardTarget(bool instant)
    {
        if (_guideTrigger == null || _guideTrigger.ActivePlayer == null) return;

        Vector3 direction = _guideTrigger.ActivePlayer.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

        transform.rotation = instant
            ? targetRotation
            : Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
}