using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Simplest IQuantumTransition: teleports the object instantly
/// to a random position within a configurable radius around canonical.
/// No shader requirements.
/// </summary>

public class InstantTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _radius = 3f;
    
    [Tooltip("Empty = use random; populated = pick from these.")] [SerializeField]
    private Vector3[] _fixedOffsets;
    
    private Vector3 _originPosition;

    private void Awake()
    {
        _originPosition = transform.position;
    }

    private void OnValidate()
    {
        if (_fixedOffsets == null || _fixedOffsets.Length == 0)
        	Debug.LogWarning("Offsets are empty, cube will not move.");
    }
    
    public SuperpositionState PickNextState(SuperpositionState current)
    {
        Vector3 offset;
        
        if (_fixedOffsets != null && _fixedOffsets.Length > 0)
        {
            offset = _fixedOffsets[UnityEngine.Random.Range(0, _fixedOffsets.Length)];
        }
        else
        {
            // offset = UnityEngine.Random.insideUnitCircle * _radius;
            // offset.y = Mathf.Abs(offset.y);
            offset = Vector3.zero;
            Debug.LogWarning("Offsets are empty, cube will not move.");
        }
        
        return new SuperpositionState
        {
            pos = _originPosition + offset,
            rot = current.rot,
            scale = current.scale
        };
    }

    public IEnumerator Execute(Transform target, SuperpositionState from, SuperpositionState to, Func<bool> isCancelled)
    {
        target.position = to.pos;
        target.rotation = to.rot;
        target.localScale = to.scale;
        yield break;
    }
}