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
[RequireComponent(typeof(SuperpositionController), typeof(QuantumGhostManager))]
public class DissolveTransition : MonoBehaviour, IQuantumTransition
{
    [SerializeField] private float _fadeDuration = 0.3f;

    private Renderer _renderer;
    private SuperpositionController _controller;
    private QuantumGhostManager _ghostManager;
    private MaterialPropertyBlock _mpb;
    
    private Vector3 _originPosition;
    private readonly List<Vector3> _fixedOffsets = new();
    private readonly List<Vector3> _remaining = new();

    private void Awake()
    {
        _originPosition = transform.position;
        _renderer = GetComponent<Renderer>();
        _controller = GetComponent<SuperpositionController>();
        _ghostManager = GetComponent<QuantumGhostManager>();
        _mpb = new MaterialPropertyBlock();
        
        _controller.OnCollapse += ResetAlpha;
        _controller.OnQuantumDeactivated += ResetAlpha;

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
        
        var solidCol = GetComponent<Collider>();
        if (solidCol != null && !_controller.IsGhostOnly)
        {
            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
        
            if (solidCol is BoxCollider solidBox)
            {
                trigger.center = solidBox.center;
                trigger.size = solidBox.size * 1.2f;
            }
            
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            var contactLogic = gameObject.AddComponent<QuantumContactCollapse>();
            contactLogic.Init(_controller);
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

    private void OnDestroy()
    {
        if (_controller)
        {
            _controller.OnCollapse -= ResetAlpha;
            _controller.OnQuantumDeactivated -= ResetAlpha;
        }
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
        
        yield return FadeAlpha(1f, 0f, isCancelled);
        if (isCancelled())
            yield break; //Canceled during fade-out; don't reposition
        
        target.position = to.pos;
        target.rotation = to.rot;
        target.localScale = to.scale;
        
        yield return FadeAlpha(0f, 1f, isCancelled);
    }

    private IEnumerator FadeAlpha(float from, float to, Func<bool> isCancelled)
    {
        float t = 0f;
        Color c = _renderer.sharedMaterial.HasProperty("_BaseColor") ? _renderer.sharedMaterial.GetColor("_BaseColor") : Color.white;
        
        while (t < _fadeDuration)
        {
            if (isCancelled()) yield break;
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / _fadeDuration);
            
            _renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor("_BaseColor", c);
            _renderer.SetPropertyBlock(_mpb);
            
            yield return null;
        }

        c.a = to;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c);
        _renderer.SetPropertyBlock(_mpb);
    }

    private void ResetAlpha()
    {
        Color c = _renderer.sharedMaterial.HasProperty("_BaseColor") ? _renderer.sharedMaterial.GetColor("_BaseColor") : Color.white;
        c.a = 1f;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c);
        _renderer.SetPropertyBlock(_mpb);
    }
    
    private void RefillBag()
    {
        _remaining.Clear();
        _remaining.AddRange(_fixedOffsets);
    }
}