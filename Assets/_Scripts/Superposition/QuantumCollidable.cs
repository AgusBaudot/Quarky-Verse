using UnityEngine;

/// <summary>
/// Optional companion to SuperpositionController for objects where
/// physical collision must reflect the collapsed state.
/// On collapse: enables or disables the Collider based on whether
/// the canonical position is meant to be passable (e.g. open door).
/// On restore: disables the Collider - an object in superposition is
/// non-solid by design.
///
/// Add this alongside SuperpositionController on any object where
/// physical interaction is state-dependent.
/// </summary>

public class QuantumCollidable : MonoBehaviour
{
    [Tooltip("True = canonical state is passable (e.g. door is open). " +
             "False = canonical state is blocking (e.g. door is closed)")]
    [SerializeField] private bool _canonicalIsPassable = false;

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        var controller = GetComponent<SuperpositionController>();
        controller.OnCollapse += HandleCollapse;
        controller.OnRestore += HandleRestore;
        controller.OnQuantumDeactivated += HandleCollapse; //Trat as collapsed
        controller.OnQuantumActivated += HandleRestore; //Resume normal state
    }

    private void OnDestroy()
    {
        var controller = GetComponent<SuperpositionController>();
        controller.OnCollapse -= HandleCollapse;
        controller.OnRestore -= HandleRestore;
        controller.OnQuantumDeactivated -= HandleCollapse;
        controller.OnQuantumActivated -= HandleRestore;
    }

    private void HandleCollapse() => _collider.enabled = !_canonicalIsPassable;
    private void HandleRestore() => _collider.enabled = false;

}