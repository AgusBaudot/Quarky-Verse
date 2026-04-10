// ObserverTrigger.cs
// ─────────────────────────────────────────────────────
// Lives on the same GameObject as SuperpositionController and
// one IQuantumTransition. Owns the offset-consequence mapping
// for this object's quantum puzzle.
//
// On collapse: activates the consequence mapped to the current
//   position, deactivates all others.
// On restore: deactivates all consequences — no position is resolved.
//
// Also exposes Observations so InstantTransition can read offsets
// from here instead of maintaining a duplicate list.
// ─────────────────────────────────────────────────────

using System.Collections.Generic;
using UnityEngine;

public class ObserverTrigger : MonoBehaviour
{
    [SerializeField] private List<QuantumObservation> _observations = new();

    public IReadOnlyList<QuantumObservation> Observations => _observations;

    SuperpositionController _controller;
    Vector3 _originPosition;

    private HashSet<GameObject> _activeConsequences = new();

    void Awake()
    {
        _originPosition = transform.position;
        _controller = GetComponent<SuperpositionController>();
    }

    void Start()
    {
        _controller.OnCollapse += HandleCollapse;
    }

    void OnDestroy()
    {
        if (_controller == null) return;
        _controller.OnCollapse -= HandleCollapse;
    }

    // transform.position is already snapped when OnCollapse fires.
    // Find the matching observation and activate only its consequence.
    void HandleCollapse()
    {
        Vector3 collapsed = transform.position;
        HashSet<GameObject> consequencesToActivate = new();

        foreach (var obs in _observations)
        {
            if (obs.consequence == null)
                continue;

            if (Vector3.Distance(collapsed, _originPosition + obs.offset) < 0.01f)
            {
                consequencesToActivate.Add(obs.consequence);
            }
        }

        foreach (var oldCon in _activeConsequences)
        {
            if (oldCon != null && !consequencesToActivate.Contains(oldCon))
            {
                oldCon.SetActive(false);
            }
        }

        foreach (var newCon in consequencesToActivate)
        {
            if (newCon != null)
            {
                newCon.SetActive(true);
            }
        }

        _activeConsequences = consequencesToActivate;
    }
}