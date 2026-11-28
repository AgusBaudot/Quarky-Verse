using UnityEngine;
using UnityEngine.Events;

public class ActionButton : MonoBehaviour, IInteractable
{
    [Header("Button Settings")]
    [SerializeField] private Transform[] _players;
    [SerializeField] private GameObject[] _targetsToDestroy;
    [SerializeField] private float _interactionRange = 2f;
    [SerializeField] private KeyCode _interactKey = KeyCode.F;

    [Header("Events")]
    [SerializeField] private UnityEvent _onPressed;

    [Header("Visual")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _inactiveColor = Color.red;
    [SerializeField] private Color _activeColor = Color.green;

    public float InteractionRange => _interactionRange;
    public bool IsActivated { get; private set; }

    private OdysseyPlayerController[] _playerControllers;

    private void Start()
    {
        if (_players != null)
        {
            _playerControllers = new OdysseyPlayerController[_players.Length];
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] != null)
                {
                    _players[i].TryGetComponent(out _playerControllers[i]);
                }
            }
        }

        UpdateVisual();
    }

    private void Update()
    {
        if (IsActivated || _players == null || _players.Length == 0) return;
        if (!Input.GetKeyDown(_interactKey)) return;

        for (int i = 0; i < _players.Length; i++)
        {
            Transform player = _players[i];
            if (player == null || !player.gameObject.activeInHierarchy) continue;

            OdysseyPlayerController controller = _playerControllers[i];
            if (controller != null)
            {
                // Ignora al personaje que no estás controlando ahora o si lleva una caja
                if (!controller.enabled || controller.IsHoldingObject) continue;
            }

            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= _interactionRange)
            {
                OnInteract();
                break;
            }
        }
    }

    public void OnInteract()
    {
        if (IsActivated) return;

        if (_targetsToDestroy == null || _targetsToDestroy.Length == 0)
        {
            Debug.LogWarning($"No hay objetos asignados para destruir en el botón {name}.", this);
            return;
        }

        IsActivated = true;

        for (int i = 0; i < _targetsToDestroy.Length; i++)
        {
            if (_targetsToDestroy[i] != null)
            {
                Destroy(_targetsToDestroy[i]);
                _targetsToDestroy[i] = null;
            }
        }

        _onPressed?.Invoke();
        UpdateVisual();
    }

    public void OnFocus() { }
    public void OnLoseFocus() { }

    private void UpdateVisual()
    {
        if (_renderer == null) return;
        Color targetColor = IsActivated ? _activeColor : _inactiveColor;
        _renderer.material.SetColor("_BaseColor", targetColor);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
}