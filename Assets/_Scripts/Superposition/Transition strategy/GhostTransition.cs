using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// IQuantumTransition that renders the object in two positions
/// simultaneously: the real object stays at a canonical (semi-transparent)
/// while a visual-only ghost appears at a designer-set alternate state.
///
/// SETUP: requires a URP Lit material with Surface Type: Transparent.
/// </summary>

//PROTOTYPE NOTE: The ghost is a minimal GameObject with MeshFilter
//and MeshRenderer only - no collider, no scripts, purely visual.

public class GhostTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private SuperpositionState _alternateState;
    [SerializeField] private float _ghostAlpha = 0.5f;

    private SuperpositionController _controller;
    private Renderer _renderer;
    private GameObject _ghost;
    private bool _isAtAlternate = false;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _controller = GetComponent<SuperpositionController>();
        _controller.OnCollapse += HandleCollapse;
        _controller.OnRestore += HandleRestore;
        _controller.OnQuantumDeactivated += HandleCollapse; //ghost hides either way
        _controller.OnQuantumActivated += HandleRestore; //ghost eligible to reappear

        _ghost = BuildGhost(transform);
        _ghost.SetActive(false);
    }

    private void OnDestroy()
    {
        _controller.OnCollapse -= HandleCollapse;
        _controller.OnRestore -= HandleRestore;
        _controller.OnQuantumDeactivated -= HandleCollapse;
        _controller.OnQuantumActivated -= HandleRestore;
    }

    public SuperpositionState PickNextState(SuperpositionState current)
    {
        //Toggle between primary (wherever the object currently is)
        //and the designer-set alternate position.
        _isAtAlternate = !_isAtAlternate;
        return _isAtAlternate ? _alternateState : current;
    }

    public IEnumerator Execute(Transform target, SuperpositionState from, SuperpositionState to, Func<bool> isCancelled)
    {
        _ghost.SetActive(true);
        SetAlpha(_renderer, _ghostAlpha);
        yield break;
    }

    private void HandleCollapse()
    {
        _isAtAlternate = false; //Reset toggle so next activation starts fresh.
        _ghost.SetActive(false);
        SetAlpha(_renderer, 1f);
    }

    private void HandleRestore()
    {
        if (!_controller.IsVisuallyQuantum) return; //Goggles restored but Planck still low, or vice versa.
        _ghost.SetActive(true);
        SetAlpha(_renderer, _ghostAlpha);
    }

    private GameObject BuildGhost(Transform source)
    {
        var ghost = new GameObject("QuantumGhost");
        var mf = ghost.AddComponent<MeshFilter>();
        var mr = ghost.AddComponent<MeshRenderer>();
        var col = ghost.AddComponent<BoxCollider>();
        mf.mesh = source.GetComponent<MeshFilter>().sharedMesh;
        mr.material = new Material(source.GetComponent<MeshRenderer>().material);
        ghost.transform.position = _alternateState.pos;
        ghost.transform.rotation = _alternateState.rot;
        ghost.transform.localScale = _alternateState.scale;
        SetAlpha(mr, _ghostAlpha);
        return ghost;
    }

    private void SetAlpha(Renderer r, float alpha)
    {
        Color c = r.material.GetColor("_BaseColor");
        c.a = alpha;
        r.material.SetColor("_BaseColor", c);
    }
}