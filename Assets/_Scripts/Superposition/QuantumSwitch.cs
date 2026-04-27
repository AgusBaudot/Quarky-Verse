using System;
using UnityEngine;

public class QuantumSwitch : MonoBehaviour
{
    [Tooltip("The SwitchActivated quantum object this switch controls.")] [SerializeField]
    private SuperpositionController _target;
    [Tooltip("World position the object collapses to when the switch fires.")]
    [SerializeField] private Vector3 _targetWorldPosition;
    [SerializeField] private float _radius;
    
    private bool _hasActivated = false;
    private bool _isInRange;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && _isInRange)
            Activate();
    }

    public void Activate()
    {
        if (_hasActivated)
            return;

        if (_target == null)
            return;
        
        _hasActivated = true;
        _target.CollapseToPosition(_targetWorldPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _isInRange = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), _radius);
    }
}