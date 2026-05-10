using UnityEngine;

//Excelent candidate for interface. Depend on abstraction, not on concrete things.
public class Highlightable : MonoBehaviour, IHighlightable
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color _highlightColor = Color.yellow;
    
    private Color _originalColor;
    private MaterialPropertyBlock _mpb;
    
    void Awake() 
    {
        if (_renderer == null) 
        {
            _renderer = GetComponent<Renderer>();
        }
        _mpb = new MaterialPropertyBlock();
        
        if (_renderer.sharedMaterial.HasProperty("_BaseColor"))
            _originalColor = _renderer.sharedMaterial.GetColor("_BaseColor");
    }

    public void SetHighlight(bool active)
    {
        Color c = active ? _highlightColor : _originalColor;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor("_BaseColor", c);
        _renderer.SetPropertyBlock(_mpb);
    }
}