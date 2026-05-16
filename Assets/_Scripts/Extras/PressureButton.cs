using UnityEngine;

public class PressureButton : MonoBehaviour
{
    [SerializeField] private Transform _pressPoint;
    [SerializeField] private float _radius = 0.3f;
    [SerializeField] private LayerMask _detectLayer;
    private bool _isPressed;
    [SerializeField] private float _requiredMass = 5f;
    [SerializeField] private Door _door;

    [SerializeField] private Transform _visual;
    [SerializeField] private float _pressedY = -0.1f;
    [SerializeField] private float _speed = 5f;
    private Vector3 _initialPos;

    void Start()
    {
        _initialPos = _visual.localPosition;
    }

    void Update()
    {
        Collider[] hits = Physics.OverlapSphere(_pressPoint.position, _radius, _detectLayer);

        float totalMass = 0f;

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.attachedRigidbody;
            if (rb != null) totalMass += rb.mass;
        }
        bool pressed = totalMass >= _requiredMass;
        if (pressed && !_isPressed)
        {
            _isPressed = true;
            OnPressed();
        }
        else if (!pressed && _isPressed)
        {
            _isPressed = false;
            OnReleased();
        }
        Vector3 target = _isPressed ? _initialPos + new Vector3(0, _pressedY, 0) : _initialPos;
        _visual.localPosition = Vector3.Lerp(_visual.localPosition, target, Time.deltaTime * _speed);
    }
    private void OnPressed()
    {
        Debug.Log("Boton Activado");
        //_door.Open();
    }
    private void OnReleased()
    {
        Debug.Log("Boton Desactivado");
        //_door.Close();
    }
}
