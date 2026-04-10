using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// IQuantumTransition that fades the object out, teleports it,
/// then fades it back in. Operates on the _BaseColor alpha channel.
///
/// SETUP: Requires a URP Lit material with Surface Type set to
/// Transparent - otherwise alpha changes will have no visible effect.
/// </summary>
public class DissolveTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _ghostAlpha = 0.4f;
    [SerializeField] private float _fadeDuration = 0.3f;
    [Tooltip("Pick position from these.")]
    [SerializeField] private Transform _ghostParent;

    private Renderer _renderer;
    private Vector3 _originPosition;
    private SuperpositionController _controller;
    private List<Vector3> _fixedOffsets = new();
    
    private List<Vector3> _remaining = new();
    private Dictionary<Vector3, GameObject> _ghosts = new();

    private void Awake()
    {
        _originPosition = transform.position;
        _renderer = GetComponent<Renderer>();

        _controller = GetComponent<SuperpositionController>();
        _controller.OnCollapse += HandleCollapse;
        _controller.OnRestore += HandleRestore;
        _controller.OnQuantumDeactivated += HandleCollapse;
        _controller.OnQuantumActivated += HandleRestore;

        var observer = GetComponent<ObserverTrigger>();
        if (observer != null)
        {
            _fixedOffsets.Clear();
            foreach (var obs in observer.Observations)
                _fixedOffsets.Add(obs.offset);
        }
        
        if (_fixedOffsets.Count == 0)
        {
            Debug.LogWarning($"{name}: DissolveTransition has no fixed offsets - object will note move.", this);
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
        _controller.OnCollapse -= HandleCollapse;
        _controller.OnRestore -= HandleRestore;
        _controller.OnQuantumDeactivated -= HandleCollapse;
        _controller.OnQuantumActivated -= HandleRestore;
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
        
        yield return FadeAlpha(1f, 0f, isCancelled);
        if (isCancelled()) yield break; //Canceled during fade-out; don't reposition
        target.position = to.pos;
        target.rotation = to.rot;
        target.localScale = to.scale;
        yield return FadeAlpha(0f, 1f, isCancelled);
    }

    private IEnumerator FadeAlpha(float from, float to, Func<bool> isCancelled)
    {
        float t = 0f;
        Color c = _renderer.material.GetColor("_BaseColor");
        while (t < _fadeDuration)
        {
            if (isCancelled()) yield break; //check BEFORE writing
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / _fadeDuration);
            _renderer.material.SetColor("_BaseColor", c);
            yield return null;
        }

        c.a = to;
        _renderer.material.SetColor("_BaseColor", c);
    }

    private void HandleCollapse()
    {
        //Coroutine has already been stopped by the controller.
        // Reset alpha so the obejct is fully visible at canonical position.
        Color c = _renderer.material.GetColor("_BaseColor");
        c.a = 1f;
        _renderer.material.SetColor("_BaseColor", c);
        
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(false);
    }
    
    private void RefillBag()
    {
        _remaining.Clear();
        _remaining.AddRange(_fixedOffsets);
    }

    private GameObject BuildGhost(Vector3 worldPos, Quaternion rot, Vector3 scale)
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
        
        ghost.transform.SetParent(_ghostParent, true);
        
        return ghost;
    }

    private void SetAlpha(Renderer r, float alpha)
    {
        Color c = r.material.GetColor("_BaseColor");
        c.a = alpha;
        r.material.SetColor("_BaseColor", c);
    }
    
    private void HandleRestore()
    {
        foreach (var ghost in _ghosts.Values)
            ghost.SetActive(true);
    }
}