using UnityEngine;
using UnityEngine.UI;

public class ObjectDetectionSystem : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Camera _camera;
    [SerializeField] private float _detectDistance = 4f;
    [SerializeField] private LayerMask _interactionMask;

    [Header("UI")]
    [SerializeField] private GameObject _interactionUI;

    private IInteractable _currentInteractable;
    private IHighlightable _currentHighlight;

    void Update()
    {
        DetectObject();
        if (Input.GetKeyDown(KeyCode.F)) Interact();
    }

    private void DetectObject()
    {
        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _detectDistance, _interactionMask))
        {
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            IHighlightable highlightable = hit.collider.GetComponentInParent<IHighlightable>();

            if (interactable != null)
            {
                if (_currentInteractable != interactable)
                {
                    ClearCurrentInteractable();
                    _currentInteractable = interactable;
                    _currentHighlight = highlightable;

                    _currentHighlight?.SetHighlight(true);
                }
                return;
            }
        }
        ClearCurrentInteractable();
    }

    private void Interact()
    {
        _currentInteractable?.OnInteract();
    }

    private void ClearCurrentInteractable()
    {
        if (_currentHighlight != null)
        {
            _currentHighlight.SetHighlight(false);
            _currentHighlight = null;
        }
        _currentInteractable = null;
    }
}