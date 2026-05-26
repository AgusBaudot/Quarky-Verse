using System.Collections;
using UnityEngine;

/// <summary>
/// Placed on the main quantum object. Restores superposition when the player
/// stops physically touching the collapsed object.
/// </summary>
[RequireComponent(typeof(SuperpositionController), typeof(Collider))]
public class QuantumContactRestorer : MonoBehaviour
{
    private SuperpositionController _controller;
    private Collider _col;
    private Coroutine _checkRoutine;

    private void Awake()
    {
        _controller = GetComponent<SuperpositionController>();
        _col = GetComponent<Collider>();
    }

    public void BeginExitCheck(CharacterController playerCC)
    {
        if (_checkRoutine != null)
            StopCoroutine(_checkRoutine);
        
        _checkRoutine = StartCoroutine(ExitCheckRoutine(playerCC));
    }

    private IEnumerator ExitCheckRoutine(CharacterController playerCC)
    {
        yield return null;

        while (!_controller.IsVisuallyQuantum)
        {
            Bounds expandedBounds = _col.bounds;
            expandedBounds.Expand(0.2f);

            if (!expandedBounds.Intersects(playerCC.bounds))
            {
                _controller.Restore();
                _checkRoutine = null;
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}