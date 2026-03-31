using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to the Player. Manages the "Gafas de Colapso" mechanic.
/// On activation: switches to first-camera camera and collapses all
/// SuperpositionControllers within radius to their canonical state.
/// After a fixed duration: restores all affected objects and returns
/// to third-person camera. The radius check is intentional design-
/// the gafas is a spatial tool, not a global toggle.
/// </summary>

//PROTOTYPE NOTE: Input is hardcoded to "E".
//Wire to Input System when the prototype is promoted.

//PROTOTYPE NOTE: Camera swap is a direct enable/disable.
//Replace with Cinemachine VCs when available.

public class CollapseGlassesController : MonoBehaviour
{
    [SerializeField] private float _duration = 5f;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private Camera _thirdPersonCamera;
    [SerializeField] private Camera _firstPersonCamera;
    [SerializeField] private Slider _timerUI;

    private bool _isActive;
    private readonly List<SuperpositionController> _collapsedObjects = new();

    private void Update()
    {
        if (!_isActive && Input.GetKeyDown(KeyCode.E))
            StartCoroutine(ActivateGlasses());
    }

    //Uses Quantum registry instead of Physics.OverlapSphere.
    //Detection is now fully independent of collider state.
    private IEnumerator ActivateGlasses()
    {
        _isActive = true;

        _thirdPersonCamera.enabled = false;
        _firstPersonCamera.enabled = true;
        
        _collapsedObjects.Clear();
        foreach (var controller in QuantumRegistry.Instance.GetInRadius(transform.position, _radius))
        {
            controller.Collapse();
            _collapsedObjects.Add(controller);
        }

        float elapsed = 0f;
        if (_timerUI) _timerUI.gameObject.SetActive(true);

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            if (_timerUI) _timerUI.value = 1f - elapsed / _duration;
            yield return null;
        }
        
        foreach (var controller in _collapsedObjects)
            controller.Restore();

        _firstPersonCamera.enabled = false;
        _thirdPersonCamera.enabled = true;
        
        if (_timerUI) _timerUI.gameObject.SetActive(false);
        _isActive = false;
    }
}
