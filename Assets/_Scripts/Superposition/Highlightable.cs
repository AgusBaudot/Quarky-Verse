using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _highlightColor = Color.yellow;
    private Color _originalColor;

    void Awake() 
    {
        if (_renderer == null) 
        {
            _renderer = GetComponent<Renderer>();
            _originalColor = _renderer.material.GetColor("_BaseColor");

        }
    }

    public void SetHighlight(bool active) 
    {
        Color c = active ? _highlightColor : _originalColor;
        _renderer.material.SetColor("_BaseColor",c);
    }
}

