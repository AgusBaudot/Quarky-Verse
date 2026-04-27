using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Core component for any object with quantum superposition behaviour.
/// Owns the interval timer, the canonical state, the current
/// apparent state, and two independent inactive flags:
///     isCollapsed - temporary, triggered by CollapseGoggles
///     isQuantumActive - persistent, driven by PlanckBar threshold
/// The superposition loop respects both. An object is only quantum
/// when active AND not collapsed.
///
/// Delegates position picking and visual transitions to IQuantumTransition.
/// </summary>
[RequireComponent(typeof(IQuantumTransition))]
public class SuperpositionController : MonoBehaviour
{
    public event Action OnCollapse;
    public event Action OnRestore;
    public event Action OnQuantumDeactivated;
    public event Action OnQuantumActivated;

    public bool IsVisuallyQuantum => _isQuantumActive && !_isCollapsed;
    public bool IsGhostOnly => _role is QuantumObjectRole.Decorative or QuantumObjectRole.SwitchActivated;
    
    [SerializeField] private float _interval = 2f;
    [SerializeField] private QuantumThreshold _minimumThreshold = QuantumThreshold.Small;
    [SerializeField] private QuantumObjectRole _role = QuantumObjectRole.PuzzleObject;

    private SuperpositionState _currentState;
    
    private bool _isCollapsed;
    private bool _isQuantumActive;

    private IQuantumTransition _transition;

    private void Awake()
    {
        _transition = GetComponent<IQuantumTransition>();
        _currentState = new SuperpositionState
        {
            pos = transform.position,
            rot = transform.rotation,
            scale = transform.localScale
        };
    }

    private void Start()
    {
        if (_role == QuantumObjectRole.PuzzleObject)
            QuantumRegistry.Instance.Register(this);
        
        //Evaluate immediately in case objects are spawned mid-session
        //with Planck level already below their threshold
        HandlePlanckChanged(PlanckBar.Instance.CurrentLevel);
        PlanckBar.Instance.OnThresholdChanged += HandlePlanckChanged;
        StartCoroutine(SuperpositionLoop());
    }

    private void OnDestroy()
    {
        if (_role == QuantumObjectRole.PuzzleObject)
            QuantumRegistry.Instance?.Unregister(this);
    
        if (PlanckBar.Instance != null)
            PlanckBar.Instance.OnThresholdChanged -= HandlePlanckChanged;
    }

    private IEnumerator SuperpositionLoop()
    {
        while (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(_interval); //Can reduce allocation with GetWait dictionary in static script.
            if (_isCollapsed || !_isQuantumActive) continue;

            var next = _transition.PickNextState(_currentState);
            
            //isCanceled is checked by the transition each frame before writing to the transform.
            yield return StartCoroutine(
                _transition.Execute(transform, _currentState, next,
                    () => _isCollapsed || !_isQuantumActive)
            );
            
            //Only advance state if the transition commpleted cleanly.
            //If cancelled mid-transition, SnapToCanonical already ran
            //and currentState stays valid for the next Execute call.
            if (!_isCollapsed && _isQuantumActive)
                _currentState = next;
        }
    }

    //Goggles triggered. Temporary - Restore() reverses this.
    public void Collapse()
    {
        _isCollapsed = true;
        SnapToCurrent();
        OnCollapse?.Invoke();
    }

    //Goggles triggered. Only resumes loop if Planck also allows it.
    public void Restore()
    {
        _isCollapsed = false;
        OnRestore?.Invoke();
    }
    
    //Checks whether this object's threshold is still met.
    // Fires deactivation/activation events only on actual state change.
    private void HandlePlanckChanged(QuantumThreshold newLevel)
    {
        bool shouldBeActive = newLevel >= _minimumThreshold;
        if (!shouldBeActive && _isQuantumActive)
        {
            _isQuantumActive = false;
            SnapToCurrent();
            OnQuantumDeactivated?.Invoke();
        }
        else if (shouldBeActive && !_isQuantumActive)
        {
            _isQuantumActive = true;
            OnQuantumActivated?.Invoke();
        }
    }

    private void SnapToCurrent()
    {
        transform.position = _currentState.pos;
        transform.rotation = _currentState.rot;
        transform.localScale =  _currentState.scale;
    }

    public void CollapseToPosition(Vector3 worldPosition)
    {
        if (_role != QuantumObjectRole.SwitchActivated)
            return;

        _role = QuantumObjectRole.PuzzleObject;
        QuantumRegistry.Instance.Register(this);

        _currentState = new SuperpositionState
        {
            pos = worldPosition,
            rot = transform.rotation,
            scale = transform.localScale
        };
        
        _isCollapsed = true;
        SnapToCurrent();
        OnCollapse?.Invoke();
        
        var renderer = GetComponent<Renderer>();
        if (!renderer.enabled)
            renderer.enabled = true;
    }
}