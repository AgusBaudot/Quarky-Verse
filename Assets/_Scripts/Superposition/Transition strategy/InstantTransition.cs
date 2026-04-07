using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Simplest IQuantumTransition: teleports the object instantly
/// to a random position within a configurable radius around canonical.
/// No shader requirements.
/// </summary>
public class InstantTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _ghostAlpha = 0.4f;
    [Tooltip("Pick position from these.")] [SerializeField]
    private List<Vector3> _fixedOffsets = new();

    private Renderer _renderer;
    private Vector3 _originPosition;
    private SuperpositionController _controller;
    
    //Shuffle bag: positions not yet visited this cycle
    private List<Vector3> _remaining = new();
    //All ghost objects keyed by world position for O(1) lookup
    private Dictionary<Vector3, GameObject> _ghosts = new();

    private void Awake()
    {
        _originPosition = transform.position;
        _renderer = GetComponent<Renderer>();
        
        var controller = GetComponent<SuperpositionController>();
        controller.OnCollapse += HandleCollapse;
        controller.OnRestore += HandleRestore;
        controller.OnQuantumDeactivated += HandleCollapse; //ghost hides either way
        controller.OnQuantumActivated += HandleRestore; //ghost eligible to reappear

        if (_fixedOffsets.Count == 0)
        {
            Debug.LogWarning($"{name}: InstantTransition has no fixed offsets - object will not move.", this);
            return;
        }
        
        //Spawn a ghost at every position. Origin intentionally excluded here.
        foreach (var offset in _fixedOffsets)
        {
            _ghosts[_originPosition + offset] = BuildGhost(_originPosition + offset, transform.rotation, transform.localScale);
        }
        
        _fixedOffsets.Add(Vector3.zero);

        RefillBag();
    }

    private void OnDestroy()
    {
        var controller = GetComponent<SuperpositionController>();
        controller.OnCollapse -= HandleCollapse;
        controller.OnRestore -= HandleRestore;
        controller.OnQuantumDeactivated -= HandleCollapse;
        controller.OnQuantumActivated -= HandleRestore;
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

        int index = UnityEngine.Random.Range(0, _remaining.Count);
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
        if (isCancelled()) yield break;
        
        //Show ghost where the real object is leaving from.
        if (!_ghosts.TryGetValue(from.pos, out var departureGhost))
        {
            //First time leaving origin - create its ghost now.
            departureGhost = BuildGhost(from.pos, from.rot, from.scale);
            _ghosts[from.pos] = departureGhost;
        }
        departureGhost.SetActive(true);
        
        //Hide ghost where the real object is arriving.
        if (_ghosts.TryGetValue(to.pos, out var arrivalGhost))
            arrivalGhost.SetActive(false);

        target.position = to.pos;
        target.rotation = to.rot;
        target.localScale = to.scale;
    }

    private void RefillBag()
    {
        _remaining.Clear();
        _remaining.AddRange(_fixedOffsets);
    }

    private GameObject BuildGhost(Vector3 worldPos, quaternion rot, Vector3 scale)
    {
        var ghost = new GameObject("QuantumGhost");
        var mf = ghost.AddComponent<MeshFilter>();
        var mr = ghost.AddComponent<MeshRenderer>();
        mf.mesh = GetComponent<MeshFilter>().sharedMesh;
        mr.material = new Material(_renderer.material);
        ghost.transform.position = worldPos;
        ghost.transform.rotation = rot;
        ghost.transform.localScale = scale;
        SetAlpha(mr, _ghostAlpha);
        return ghost;
    }

    private void SetAlpha(Renderer r, float alpha)
    {
        Color c = r.material.GetColor("_BaseColor");
        c.a = alpha;
        r.material.SetColor("_BaseColor", c);
    }

    private void HandleCollapse()
    {
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(false);
    }
    
    private void HandleRestore()
    {
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(true);
    }
}