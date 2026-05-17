using System;
using UnityEngine;

/// <summary>
/// Injected into ghosts. Forces the main quantum object to collapse
/// at this specific ghost's position upon player contact.
/// </summary>
public class QuantumContactCollapse : MonoBehaviour
{
    private SuperpositionController _controller;
    private string _playerTag = "Player";

    /// <summary>
    /// Called by QuantumGhostManager when generating a ghost via code.
    /// </summary>
    /// <param name="controller"></param>
    public void Init(SuperpositionController controller)
    {
        _controller = controller;
    }

    //Fired if standard rb.
    private void OnTriggerEnter(Collider other)
    {
        EvaluateContact(other.gameObject);
    }

    private void EvaluateContact(GameObject interactor)
    {
        if (_controller == null || !_controller.IsVisuallyQuantum)
            return;

        if (!interactor.CompareTag(_playerTag))
            return;

        _controller.SetCanonicalPosition(transform.position);
        _controller.Collapse();

        var restorer = _controller.GetComponent<QuantumContactRestorer>();
        var playerCC = interactor.GetComponent<CharacterController>();
        if (restorer != null && playerCC != null)
        {
            restorer.BeginExitCheck(playerCC);
        }
    }
}