using UnityEngine;

[RequireComponent(typeof(EntanglementNode))]
public class EntanglementMirror : MonoBehaviour
{
    [Tooltip("If true, this object moves in the exact opposite direction of the received movement.")]
    [SerializeField] private bool _invertMovement = false;
    
    private EntanglementNode _node;
    private Vector3 _lastPosition;
    private bool _isApplyingSync = false;

    private void Awake()
    {
        _node = GetComponent<EntanglementNode>();
        _lastPosition = transform.position;
    }

    private void OnEnable() => _node.OnMirrorSync += HandleMirrorSync;
    private void OnDisable() => _node.OnMirrorSync -= HandleMirrorSync;

    private void LateUpdate()
    {
        if (_isApplyingSync) return; 

        Vector3 delta = transform.position - _lastPosition;
        
        if (delta.sqrMagnitude > 0.0001f) 
        {
            EntanglementNetwork.BroadcastMirrorMovement(_node.GroupID, delta, _node);
        }
        
        _lastPosition = transform.position;
    }

    private void HandleMirrorSync(Vector3 delta)
    {
        _isApplyingSync = true;

        Vector3 finalDelta = _invertMovement ? -delta : delta;
        
        transform.position += finalDelta;
        _lastPosition = transform.position;

        _isApplyingSync = false;
    }
}