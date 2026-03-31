using System;
using System.Collections;
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
    [SerializeField] private float _radius = 3f;
    [SerializeField] private float _fadeDuration = 0.4f;

    [Tooltip("Empty = use random; populated = pick from these.")] [SerializeField]
    private Vector3[] _fixedOffsets;

    private Renderer _renderer;
    private SuperpositionController _controller;
    private Vector3 _originPosition;
    
    private void Awake()
    {
        _originPosition = transform.position;
        
        _renderer = GetComponent<Renderer>();
        _controller = GetComponent<SuperpositionController>();
        _controller.OnCollapse += HandleCollapse;
        _controller.OnQuantumDeactivated += HandleCollapse;
    }

    private void OnDestroy()
    {
        _controller.OnCollapse -= HandleCollapse;
        _controller.OnQuantumDeactivated -= HandleCollapse;
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
            // offset = Random.insideUnitCircle * _radius;
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
    }
}