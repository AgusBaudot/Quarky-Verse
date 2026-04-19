using UnityEngine;

public class PressureButton : MonoBehaviour
{
    [SerializeField] private Transform _pressPoint;
    [SerializeField] private float _radius = 0.3f;
    [SerializeField] private LayerMask _detectLayer;
    private bool _isPressed;
    [SerializeField] private Door _door;

    void Update()
    {
        bool pressed = Physics.CheckSphere(_pressPoint.position, _radius, _detectLayer);
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
    }
    private void OnPressed()
    {
        Debug.Log("Boton Activado");
        _door.Open();
    }
    private void OnReleased()
    {
        Debug.Log("Boton Desactivado");
        _door.Close();
    }
}
