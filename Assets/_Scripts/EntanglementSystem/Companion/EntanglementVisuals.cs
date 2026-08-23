using UnityEngine;

[RequireComponent(typeof(EntanglementNode))]
public class EntanglementVisuals : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private Color _idleColor = Color.gray;
    [SerializeField] private Color _activeColor = Color.cyan;

    private EntanglementNode _node;
    private Material _mat;

    private void Awake()
    {
        _node = GetComponent<EntanglementNode>();
        if (_renderer != null) _mat = _renderer.material;
    }

    private void OnEnable() => _node.OnQuantumSync += HandleSync;
    private void OnDisable() => _node.OnQuantumSync -= HandleSync;

    private void HandleSync(EntanglementEvent syncEvent)
    {
        if (_mat == null) return;

        if (_node.CurrentState == EntanglementState.Activated)
            _mat.SetColor("_BaseColor", _activeColor);
        else
            _mat.SetColor("_BaseColor", _idleColor);
    }
}