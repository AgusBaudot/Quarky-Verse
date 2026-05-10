using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Simplest IQuantumTransition: teleports the object instantly
/// to a random position within a configurable radius around canonical.
/// No shader requirements.
/// </summary>
[RequireComponent(typeof(SuperpositionController), typeof(QuantumGhostManager))]
public class InstantTransition : MonoBehaviour, IQuantumTransition
{
    private Renderer _renderer;
    private SuperpositionController _controller;
    private QuantumGhostManager _ghostManager;
    private Vector3 _originPosition;
    
    private readonly List<Vector3> _fixedOffsets = new();
    //Shuffle bag: positions not yet visited this cycle
    private readonly List<Vector3> _remaining = new();

    private void Awake()
    {
        _originPosition = transform.position;
        _renderer = GetComponent<Renderer>();
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
            Debug.LogWarning($"{name}: InstantTransition has no fixed offsets - object will not move.", this);
            return;
        }
        
        _ghostManager.InitializeGhosts(_fixedOffsets, _originPosition);

        if (_controller.IsGhostOnly)
        {
            _renderer.enabled = false;
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
            Debug.LogWarning($"{name}: Offsets are empty, object will not move.", this);
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
    /// Hides ghost at destination (real object arriving),
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

        target.position = to.pos;
        target.rotation = to.rot;
        target.localScale = to.scale;
    }

    private void RefillBag()
    {
        _remaining.Clear();
        _remaining.AddRange(_fixedOffsets);
    }
}