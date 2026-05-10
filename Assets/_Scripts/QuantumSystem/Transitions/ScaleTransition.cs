using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// IQuantumTransition that shrinks the object to zero scale,
/// repositions it, then grows it back to its canonical scale.
/// No shader requirements - pure Transform manipulation.
/// </summary>
[RequireComponent(typeof(SuperpositionController), typeof(QuantumGhostManager))]
public class ScaleTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _scaleDuration = 0.3f;

    private SuperpositionController _controller;
    private QuantumGhostManager _ghostManager;
    private Vector3 _originPosition;
    
    private readonly List<Vector3> _remaining = new();
    private readonly List<Vector3> _fixedOffsets = new();

    private void Awake()
    {
        _originPosition = transform.position;
        _controller = GetComponent<SuperpositionController>();
        _ghostManager = GetComponent<QuantumGhostManager>();

        var observer = GetComponent<ObserverTrigger>();
        if (observer != null)
        {
            _fixedOffsets.Clear();
            foreach (var obs in observer.Observations)
                _fixedOffsets.Add(obs.offset);
        }

        if (_fixedOffsets.Count == 0)
        {
            Debug.LogWarning($"{name}: ScaleTransition has no fixed offsets - object will not move.", this);
            return;
        }
        
        _ghostManager.InitializeGhosts(_fixedOffsets, _originPosition);

        if (_controller.IsGhostOnly)
        {
            if (TryGetComponent<Renderer>(out var r))
                r.enabled = false;
            _ghostManager.EnsureGhostExists(_originPosition, transform.rotation, transform.localScale);
        }
        else
        {
            _ghostManager.HideGhostAt(_originPosition);
        }
        
        RefillBag();
    }

    private void OnValidate()
    {
        if (_fixedOffsets == null || _fixedOffsets.Count == 0)
            Debug.LogWarning("Offsets are empty, cube will not move.");
    }

    public SuperpositionState PickNextState(SuperpositionState current)
    {
        if (_remaining.Count == 0)
            RefillBag();
        
        int index = Random.Range(0, _remaining.Count);
        var offset = _remaining[index];
        _remaining.RemoveAt(index);

        return new SuperpositionState
        {
            pos = _originPosition + offset,
            rot = current.rot,
            scale = current.scale
        };
    }

    /// <summary>
    /// Hides a ghost at destination (real object arriving),
    /// shows ghost at departure (real object leaving).
    /// </summary>
    /// <param name="target"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="isCancelled"></param>
    /// <returns></returns>
    public IEnumerator Execute(Transform target, SuperpositionState from, SuperpositionState to, Func<bool> isCancelled)
    {
        if (isCancelled() || _controller.IsGhostOnly)
            yield break;
        
        _ghostManager.EnsureGhostExists(from.pos, from.rot, from.scale);
        _ghostManager.ShowGhostAt(from.pos);
        _ghostManager.HideGhostAt(to.pos);
        
        yield return ScaleTo(target, from.scale, Vector3.zero, isCancelled);
        if (isCancelled())
            yield break; //Don't reposition if cancelled mid-shrink
        
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
    
    private void RefillBag()
    {
        _remaining.Clear();
        _remaining.AddRange(_fixedOffsets);
    }
}