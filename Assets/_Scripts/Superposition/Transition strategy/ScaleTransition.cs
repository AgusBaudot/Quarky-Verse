using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// IQuantumTransition that shrinks the object to zero scale,
/// repositions it, then grows it back to its canonical scale.
/// No shader requirements - pure Transform manipulation.
/// </summary>

public class ScaleTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _radius = 3f;
    [SerializeField] private float _scaleDuration = 0.3f;
    
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
            offset = _fixedOffsets[Random.Range(0, _fixedOffsets.Length)];
        }
        else
        {
            offset = Vector3.zero;
            // offset = Random.insideUnitCircle * _radius;
            // offset.y = Mathf.Abs(offset.y);
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
        yield return ScaleTo(target, from.scale, Vector3.zero, isCancelled);
        if (isCancelled()) yield break; //Don't reposition if cancelled mid-shrink
        target.position = to.pos;
        target.rotation = to.rot;
        yield return ScaleTo(target, Vector3.zero, to.scale, isCancelled);
    }

    private IEnumerator ScaleTo(Transform target, Vector3 from, Vector3 to, Func<bool> isCancelled)
    {
        float t = 0f;
        while (t < _scaleDuration)
        {
            if (isCancelled()) yield break; //Check BEFORE writing
            t += Time.deltaTime;
            target.localScale = Vector3.Lerp(from, to, t / _scaleDuration);
            yield return null;
        }

        target.localScale = to;
    }
}